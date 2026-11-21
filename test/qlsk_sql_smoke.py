import base64
import os
from concurrent.futures import ThreadPoolExecutor
from urllib import request as http_request
from urllib.error import HTTPError
from urllib.parse import urlsplit
import json
import uuid
from datetime import datetime, timedelta, timezone
from io import BytesIO
from zipfile import ZipFile
from xml.etree import ElementTree
from playwright.sync_api import sync_playwright, expect

BASE = os.environ["QLSK_SMOKE_URL"].rstrip("/")
if urlsplit(BASE).hostname not in {"localhost", "127.0.0.1", "::1"} or os.environ.get("QLSK_SMOKE_DEDICATED_DB") != "1":
    raise SystemExit("Run against a local QLSK instance connected to a dedicated test database.")
TMP = uuid.uuid4().hex[:8]
NOW = datetime.now(timezone.utc)
HCM = timezone(timedelta(hours=7))
png = base64.b64decode("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAOksrOYAAAAASUVORK5CYII=")
ADMIN = os.environ["QLSK_SMOKE_ADMIN_EMAIL"]
ADMIN_PASSWORD = os.environ["QLSK_SMOKE_ADMIN_PASSWORD"]
PASSWORD = "Qlsk-Fixture-" + uuid.uuid4().hex

def when(hours):
    return (NOW + timedelta(hours=hours)).astimezone(HCM).isoformat(timespec="seconds")

def caps(client):
    response = client.get("/api/capabilities")
    assert response.ok, response.text()
    return response.json()

def send(client, method, path, data=None, expected=200, multipart=None, use_csrf=True):
    headers = {"X-CSRF-Token": caps(client)["csrfToken"]} if use_csrf and method != "GET" else {}
    kwargs = dict(method=method, headers=headers)
    if data is not None:
        kwargs["data"] = data
    if multipart is not None:
        kwargs["multipart"] = multipart
    response = client.fetch("/api/"+path, **kwargs)
    assert response.status == expected, (method,path,response.status,response.text()[:1000],expected)
    if expected == 204:
        return None
    if expected >= 400:
        return response
    if "application/json" in response.headers.get("content-type",""):
        return response.json()
    return response

def create(client, path, data):
    return send(client,"POST",path,data,201)["id"]

def login(client, email, password=PASSWORD):
    send(client,"POST","auth/session",{"identifier":email,"password":password},204)
    assert caps(client)["authenticated"]

def enroll(client, account, policy):
    email=f"{account}.{TMP}@example.test"
    create(client,"auth/register",{"name":f"Test {account} {TMP}","userName":f"{account}_{TMP}",
        "email":email,"password":PASSWORD,"acceptedPolicyId":policy})
    login(client,email)
    return email

with sync_playwright() as p:
    admin = p.request.new_context(base_url=BASE)
    guest = p.request.new_context(base_url=BASE)
    organizer = p.request.new_context(base_url=BASE)
    buyer = p.request.new_context(base_url=BASE)
    other = p.request.new_context(base_url=BASE)
    assert not caps(guest)["authenticated"]
    send(guest,"POST","auth/session",{"identifier":ADMIN,"password":ADMIN_PASSWORD},403,use_csrf=False)
    assert guest.get("/api/me").status == 401
    login(admin,ADMIN,ADMIN_PASSWORD)
    assert caps(admin)["workspace"]["role"] == "admin"
    policy_ids = {}
    for title in ["Điều khoản sử dụng","Chính sách đặt vé","Chính sách thuê địa điểm"]:
        id=create(admin,"admin/catalog/policies",{"title":title,"type":"test","content":"Điều khoản kiểm thử SQL, không dùng sản xuất.", "effectiveFrom":when(-24)})
        send(admin,"PUT",f"admin/catalog/policies/{id}",{"action":"publish","expectedState":"DRAFT","reason":"Kiểm thử riêng"})
        policy_ids[title]=id
    organizer_email=enroll(organizer,"organizer",policy_ids["Điều khoản sử dụng"])
    buyer_email=enroll(buyer,"customer",policy_ids["Điều khoản sử dụng"])
    enroll(other,"other",policy_ids["Điều khoản sử dụng"])
    send(buyer,"GET","admin/accounts",expected=403)
    user = send(buyer,"GET","me")
    assert user["email"] == buyer_email
    send(buyer,"PUT","me",{"name":"Hồ sơ đã sửa","email":buyer_email})
    assert send(buyer,"GET","me")["name"] == "Hồ sơ đã sửa"
    category = create(admin,"admin/catalog/event-categories",{"name":"Chủ đề kiểm thử "+TMP})
    venue = create(admin,"admin/catalog/venues",{"name":"Địa điểm "+TMP,"address":"Địa chỉ kiểm thử, Hà Nội",
        "capacity":60,"rate":1000000,"amenities":"Điều hòa"})
    zone = create(admin,"admin/catalog/zones",{"name":"Khu A","venueId":venue,"capacity":30,"seating":"ASSIGNED","seatLayout":"A-01,A-02,A-03"})
    assert send(guest,"GET",f"venues/{venue}")["zones"][0]["name"] == "Khu A"

    file={"name":"qa.png","mimeType":"image/png","buffer":png}
    dossier = send(organizer,"POST","me/organizer-applications",expected=201,multipart={
        "registrationType":"individual","fullName":"Tên kiểm thử", "identityType":"card", "identityNumber":"000000000001",
        "issuedDate":(NOW.date()-timedelta(days=30)).isoformat(),"issuedPlace":"Nơi cấp kiểm thử", "email":organizer_email,"phone":"0912345671",
        "contactAddress":"Địa chỉ fixture", "bankName":"Ngân hàng fixture", "bankAccount":"123456789","bankOwner":"Ten kiem thu",
        "agreement":"on","identityFront":file,"identityBack":file})["id"]
    assert send(organizer,"GET",f"organizer-applications/{dossier}")["status"]=="PENDING"
    queue = send(admin,"GET","organizer-applications?status=PENDING")["items"]
    assert any(x["id"] == dossier for x in queue)
    doc_id=send(admin,"GET",f"organizer-applications/{dossier}")["documents"][0]["id"]
    send(other,"GET",f"organizer-applications/{dossier}/documents/{doc_id}",expected=404)
    assert send(admin,"GET",f"organizer-applications/{dossier}/documents/{doc_id}").body() == png
    send(admin,"POST",f"organizer-applications/{dossier}/decisions",
        {"action":"approve","expectedState":"PENDING","reason":"Giấy tờ kiểm thử đạt"})
    assert caps(organizer)["workspace"]["role"] == "organizer"
    assert any("Ban tổ chức" in n["title"] for n in send(organizer,"GET","me/notifications")["items"])

    rental_base = {"venueId":venue,"from":when(120),"to":when(124),"capacity":10,"policyId":policy_ids["Chính sách thuê địa điểm"],
        "eventName":"Họp kiểm thử","eventType":"Hội thảo","reason":"Kiểm tra thuê và giữ lịch","program":"Kiểm thử",
        "representative":"Người liên hệ thử","phone":"0912345671"}
    rental = create(buyer,"venue-rentals", rental_base)
    competing = create(other,"venue-rentals",rental_base)
    send(other,"GET",f"venue-rentals/{rental}",expected=404)
    approve={"action":"approve","expectedState":"REQUESTED","reason":"Địa điểm còn lịch"}
    result=send(admin,"POST",f"venue-rentals/{rental}/decisions",approve)
    assert result["status"]=="AWAITING_DEPOSIT" and result["depositAmount"]==300000
    send(admin,"POST",f"venue-rentals/{competing}/decisions",approve,409)
    assert len(send(guest,"GET",f"venues/{venue}/availability?from="+when(119).replace("+","%2B")+"&to="+when(125).replace("+","%2B"))["items"]) == 1
    send(buyer,"POST",f"venue-rentals/{rental}/cancellations",{"reason":"Kiểm thử trả lịch"})
    support=create(buyer,"support-cases",{"subject":"Cần hỗ trợ kiểm thử","description":"Nội dung hỗ trợ kiểm thử"})
    send(admin,"PUT",f"support-cases/{support}",{"action":"resolve","expectedState":"OPEN","reason":"Đã kiểm tra"})
    assert send(buyer,"GET",f"support-cases/{support}")["status"]=="RESOLVED"

    event = create(organizer,"manage/events",{"name":f"Sự kiện SQL {TMP}","categoryId":category,
        "description":"Hiển thị dữ liệu từ SQL Server", "program":"Kiểm thử công bố", "rules":"Xuất trình vé", "policy":"Vé miễn phí"})
    send(guest,"GET",f"events/{event}",expected=404)
    version=send(organizer,"GET",f"manage/events/{event}/versions")["items"][0]["id"]
    slot=create(organizer,f"manage/events/{event}/sessions",{"versionId":version,"name":"Suất SQL",
        "venueId":venue,"from":when(48),"to":when(50),"salesFrom":when(-1),"salesTo":when(47),"capacity":20})
    ticket_type=create(organizer,f"sessions/{slot}/ticket-types",{"versionId":version,"zoneId":zone,
        "type":"Standard","price":0,"quantity":3})
    send(organizer,"POST",f"manage/events/{event}/versions/{version}/submissions",{"expectedState":"DRAFT","reason":"Đã chuẩn bị"})
    send(organizer,"POST",f"manage/events/{event}/versions/{version}/decisions",
        {"action":"approve","expectedState":"PENDING","reason":"Tự duyệt"},403)
    send(admin,"POST",f"manage/events/{event}/versions/{version}/decisions",{"action":"approve","expectedState":"PENDING","reason":"Đạt yêu cầu"})
    publication = f"manage/events/{event}/publication"
    assert send(organizer,"GET",f"manage/events/{event}")["status"] == "APPROVED_PRIVATE"
    send(guest,"GET",f"events/{event}",expected=404)
    send(buyer,"GET",f"sessions/{slot}/inventory",expected=404)
    send(other,"GET",publication,expected=403)
    preview = send(organizer,"GET",publication)
    assert preview["versionId"] == version
    assert preview["sessions"][0]["prices"][0]["price"] == 0
    publish = {"versionId":version,"reason":"Đã kiểm tra lịch và giá vé"}
    send(admin,"POST",publication,publish,403)
    send(organizer,"POST",publication+"/reopen",publish)
    send(organizer,"POST",publication,publish,409)
    send(organizer,"POST",f"manage/events/{event}/versions/{version}/submissions",{"expectedState":"DRAFT","reason":"Đã kiểm tra lại"})
    send(admin,"POST",f"manage/events/{event}/versions/{version}/decisions",{"action":"approve","expectedState":"PENDING","reason":"Đạt yêu cầu"})
    send(guest,"GET",f"events/{event}",expected=404)
    send(organizer,"POST",publication,publish)
    send(organizer,"POST",publication,publish)
    event_json = send(guest,"GET",f"events/{event}")
    assert event_json["sessions"][0]["name"] == "Suất SQL"
    assert event_json["program"] == "Kiểm thử công bố"
    assert any(x["id"] == event for x in send(guest,"GET",f"events?q={TMP}")["items"])
    assert any(x["id"] == event for x in send(guest,"GET",f"events?q={TMP}&availability=available")["items"])
    sessions = send(guest,"GET","sessions?from="+when(47).replace("+","%2B")+"&to="+when(51).replace("+","%2B")+"&city=H%C3%A0%20N%E1%BB%99i")
    assert any(x["id"] == slot for x in sessions["items"])
    banner = create(admin,"admin/catalog/banners",{"name":"Banner SQL "+TMP,"image":"/assets/images/hero.webp","target":f"#event/{event}"})
    send(admin,"PUT",f"admin/catalog/banners/{banner}",{"action":"show","expectedState":"HIDDEN","reason":"Kiểm thử công bố"})
    assert any(x["id"] == event for x in send(guest,"GET",f"events?featured=true&q={TMP}")["items"])
    send(buyer,"PUT",f"me/favorites/{event}",expected=204)
    assert send(buyer,"GET","me/favorites")["items"][0]["id"] == event

    info = send(buyer,"GET",f"sessions/{slot}/inventory")
    seat=info["zones"][0]["seats"][0]["id"]
    assert info["remainingQuota"]==10
    assert info["zones"][0]["seats"][0]["ticketTypeId"]==ticket_type
    body={"sessionId":slot,"seatIds":[seat],"items":[{"ticketTypeId":ticket_type,"seatId":seat}],
        "policyId":info["policyId"],"idempotencyKey":str(uuid.uuid4())}
    order = create(buyer,"orders",body)
    assert create(buyer,"orders",body) == order
    send(other,"POST","orders",{**body,"idempotencyKey":str(uuid.uuid4())},409)
    send(other,"GET",f"orders/{order}",expected=404)
    assert send(buyer,"GET",f"orders/{order}")["total"] == 0
    ticket=send(buyer,"GET","me/tickets")["items"][0]["id"]
    assert send(buyer,"GET",f"tickets/{ticket}/qr").body().startswith(b"\x89PNG")
    assert send(buyer,"GET",f"tickets/{ticket}/pdf").body().startswith(b"%PDF")
    send(other,"GET",f"tickets/{ticket}/qr",expected=404)
    send(other,"POST",f"sessions/{slot}/check-ins",{"ticketId":ticket,"method":"MANUAL"},403)
    lookup=send(organizer,"POST",f"sessions/{slot}/attendees/lookup",{"query":buyer_email})
    assert lookup["items"][0]["ticketId"]==ticket
    state=send(organizer,"POST",f"sessions/{slot}/check-ins",{"ticketId":ticket,"method":"MANUAL"})
    again=send(organizer,"POST",f"sessions/{slot}/check-ins",{"ticketId":ticket,"method":"MANUAL"})
    assert state["checkedInAt"]==again["checkedInAt"]
    assert len(send(organizer,"GET",f"sessions/{slot}/check-ins")["items"])==1
    second_seat = info["zones"][0]["seats"][1]["id"]
    alternatives = []
    for claimant in [buyer, other]:
        token=caps(claimant)["csrfToken"]
        cookie="; ".join(f"{item['name']}={item['value']}" for item in claimant.storage_state()["cookies"])
        alternative={**body,"seatIds":[second_seat],"items":[{"ticketTypeId":ticket_type,"seatId":second_seat}],
            "idempotencyKey":str(uuid.uuid4())}
        alternatives.append(http_request.Request(BASE+"/api/orders",json.dumps(alternative).encode(),
            {"X-CSRF-Token":token,"Cookie":cookie,"Content-Type":"application/json"}))
    def compete(req):
        try:
            with http_request.urlopen(req,timeout=15) as response:
                return response.status
        except HTTPError as error:
            return error.code
    with ThreadPoolExecutor(2) as pool:
        assert sorted(pool.map(compete, alternatives)) == [201,409]
    payments_path = "payments?status=SUCCESS"
    payments = send(admin, "GET", payments_path)
    while not any(row["sourceLabel"] == f"Đơn #{order}" for row in payments["items"]):
        cursor = payments["nextCursor"]
        assert cursor, f"No successful payment for order {order}"
        payments = send(admin, "GET", f"{payments_path}&cursor={cursor}")
    contract=create(admin,"manage/contracts",{"organizerId":dossier,"feeType":"PERCENT","feeValue":9.5,
        "from":when(-2),"to":when(200)})
    assert send(organizer,"GET",f"manage/contracts/{contract}")["status"] == "CoHieuLuc"
    send(other,"GET",f"manage/contracts/{contract}",expected=403)
    report=send(organizer,"GET",f"reports?type=checkins&eventId={event}&sessionId={slot}")
    assert len(report["rows"])==1 and report["allowedActions"] == ["export"]
    rentals_report=send(admin,"GET","reports?type=rentals&status=CANCELLED")
    assert any(row[0] == str(rental) and row[-1] == "Đã hủy" for row in rentals_report["rows"])
    send(other,"GET",f"reports?type=checkins&eventId={event}&sessionId={slot}",expected=403)
    for format, member in [("xlsx","xl/worksheets/sheet1.xml"),("docx","word/document.xml")]:
        file=send(organizer,"GET",f"reports/export?type=checkins&eventId={event}&sessionId={slot}&format={format}")
        with ZipFile(BytesIO(file.body())) as archive:
            root=ElementTree.fromstring(archive.read(member))
            assert len(list(root.iter())) > 8
            assert "QLSK-" in archive.read(member).decode("utf-8")
    manager = send(other,"GET","me")
    send(admin,"PUT",f"admin/accounts/{manager['id']}",{"action":"assign","expectedState":"ACTIVE","permissionGroupId":1,
        "reason":"Kiểm thử phạm vi quản lý"})
    send(other,"GET","me",expected=401)
    login(other,manager["email"])
    assert caps(other)["workspace"]["role"] == "manager"
    assert "work.applications.decide" in caps(other)["operations"] and "work.scan.confirm" not in caps(other)["operations"]
    send(other,"GET","admin/accounts",expected=403)
    print("SQL finance + reports: existing receipts, scoped contracts, export XLSX and DOCX validated.")
    print("Real SQL Server: auth and CSRF, catalog and policy, organizer file/approval, rental conflict/deposit/cancel, support, event publication and calendar, favorite, idempotent booking, QR/PDF and repeated check-in OK")

    browser=p.chromium.launch(executable_path="C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe")
    session_date = when(48)[:10]
    calendar_route = f"#calendar/{session_date[:7]}/{session_date}"
    for width,height in [(1440,900),(768,1000),(375,812)]:
        context=browser.new_context(viewport={"width":width,"height":height}, storage_state=buyer.storage_state())
        page=context.new_page()
        errors=[]
        page.on("pageerror",lambda err: errors.append(str(err)))
        for fragment in ["#home", "#events", f"#event/{event}/{slot}", "#calendar", f"#venue/{venue}", "#account/profile",
                         "#tickets/mine","#requests/rentals",f"#requests/support/{support}"]:
            page.goto(BASE+"/"+fragment)
            expect(page.locator("main h1")).to_be_visible()
            assert page.evaluate("document.documentElement.scrollWidth <= innerWidth"), (fragment,width)
        page.goto(BASE+f"/#event/{event}/{slot}")
        expect(page.get_by_role("heading",name=f"Sự kiện SQL {TMP}")).to_be_visible()
        expect(page.get_by_role("heading",name="Quy định tham dự và hoàn vé")).to_be_visible()
        page.get_by_role("button",name="Chia sẻ suất diễn").click()
        expect(page.get_by_role("dialog")).to_be_visible()
        page.keyboard.press("Escape")
        expect(page.get_by_role("button",name="Chia sẻ suất diễn")).to_be_focused()
        assert "QLSK" in page.title()

        page.goto(BASE+f"/#venue/{venue}")
        expect(page.locator("#venue-title")).to_contain_text(TMP)
        expect(page.get_by_text("Ảnh minh họa", exact=True)).to_be_visible()
        page.get_by_role("button",name="Xem lịch địa điểm").click()
        expect(page.locator("#venue-availability")).to_be_focused()
        page.locator(f'.venue-day[href$="date={session_date}"]').click()
        expect(page.locator(".venue-day[aria-current=date]")).to_have_count(1)
        page.reload()
        expect(page.locator(".venue-day[aria-current=date]")).to_have_count(1)
        page.get_by_role("link",name="Mở biểu mẫu thuê").click()
        expect(page.get_by_label("Địa điểm thuê")).to_have_value(str(venue))
        expect(page.get_by_label("Bắt đầu")).to_have_value(session_date+"T09:00")
        page.go_back()
        expect(page.locator(".venue-day[aria-current=date]")).to_have_count(1)

        page.goto(BASE+"/"+calendar_route)
        agenda = page.locator(".agenda-card").filter(has=page.locator(f'a[href="#event/{event}/{slot}"]'))
        expect(agenda).to_be_visible()
        assert page.evaluate("document.documentElement.scrollWidth <= innerWidth"), (calendar_route,width)
        share = agenda.get_by_role("button",name="Chia sẻ")
        share.click()
        expect(page.get_by_role("dialog").get_by_label("Liên kết suất diễn")).to_have_value(BASE+f"/#event/{event}/{slot}")
        page.keyboard.press("Escape")
        expect(share).to_be_focused()
        with page.expect_download() as ics:
            agenda.get_by_role("button",name="Tải ICS").click()
        assert "BEGIN:VEVENT" in ics.value.path().read_text(encoding="utf-8")
        agenda.get_by_role("link",name=f"Sự kiện SQL {TMP}").click()
        expect(page.locator("#event-title")).to_have_text(f"Sự kiện SQL {TMP}")
        page.go_back()
        expect(agenda).to_be_visible()
        page.go_forward()
        expect(page.locator("#event-title")).to_have_text(f"Sự kiện SQL {TMP}")

        page.goto(BASE+f"/#export?type=checkins&eventId={event}&sessionId={slot}")
        page.get_by_role("button",name="Chọn định dạng xuất").click()
        expect(page.get_by_role("button",name="Xuất báo cáo")).to_be_disabled()
        page.keyboard.press("Escape")
        expect(page.get_by_role("button",name="Chọn định dạng xuất")).to_be_focused()
        assert not errors, errors
        context.close()

        operations = browser.new_context(viewport={"width":width,"height":height},storage_state=organizer.storage_state())
        page = operations.new_page()
        page.goto(BASE+"/#work")
        expect(page.get_by_role("heading",name="Việc cần xử lý")).to_be_visible()
        expect(page.locator(".work-metrics")).to_contain_text("Sự kiện trong phạm vi")
        expect(page.get_by_role("link",name="Phân quyền",exact=True)).to_have_count(0)
        page.get_by_role("link",name="Mở báo cáo").click()
        expect(page.get_by_role("heading",name="Phạm vi báo cáo")).to_be_visible()
        report_route=f"#export?type=checkins&eventId={event}&sessionId={slot}"
        page.goto(BASE+"/"+report_route)
        expect(page.locator(".report-preview table")).to_contain_text("Thành công")
        assert page.evaluate("document.documentElement.scrollWidth <= innerWidth"), (report_route,width)
        page.get_by_label("Từ ngày").fill("2027-01-02")
        page.get_by_label("Đến ngày").fill("2027-01-01")
        page.get_by_role("button",name="Lọc báo cáo").click()
        expect(page.get_by_label("Đến ngày")).to_have_attribute("aria-invalid","true")
        expect(page.get_by_label("Đến ngày")).to_be_focused()
        page.reload()
        expect(page.locator(".report-preview table")).to_be_visible()
        trigger = page.get_by_role("button",name="Chọn định dạng xuất")
        trigger.click()
        expect(page.get_by_role("dialog")).to_be_visible()
        page.get_by_role("radio",name="Word (.docx)").check()
        with page.expect_download() as doc:
            page.get_by_role("button",name="Xuất báo cáo").click()
        with ZipFile(doc.value.path()) as report_zip:
            assert "word/document.xml" in report_zip.namelist()
        if width == 1440:
            page.evaluate("""() => {
              const open = window.open.bind(window);
              window.open = (...args) => {
                const popup = open(...args);
                if (popup) popup.print = () => { popup.document.body.dataset.printRequested = "yes"; };
                return popup;
              };
            }""")
            page.get_by_role("radio",name="In hoặc lưu PDF").check()
            with page.expect_popup() as printed:
                page.get_by_role("button",name="Xuất báo cáo").click()
            expect(printed.value.locator("body")).to_have_attribute("data-print-requested","yes")
            expect(printed.value.locator("table")).to_contain_text("Thành công")
            printed.value.close()
        page.keyboard.press("Escape")
        expect(trigger).to_be_focused()
        operations.close()
    page=browser.new_page()
    for note in ["", "giaodienmau.html"]:
        page.goto(BASE+"/giaodientest/"+note)
        expect(page.locator("main h1")).to_contain_text("Tìm sự kiện")
    print("Edge: Desktop, tablet, mobile, QLSK venue/calendar/rentals/reports and both prototype URLs OK")
