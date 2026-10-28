# API chính

## Auth / account
POST /api/auth/register
POST /api/auth/login
POST /api/auth/logout
PUT /api/auth/change-password
GET /api/accounts/me
PUT /api/accounts/me
GET /api/accounts (admin)
POST /api/accounts/{id}/lock (admin)
POST /api/accounts/{id}/unlock (admin)
PUT /api/accounts/{id}/role (admin)

## Xác thực BTC
POST /api/organizer-verifications
GET /api/organizer-verifications/me
GET /api/organizer-verifications
POST /api/organizer-verifications/{id}/approve
POST /api/organizer-verifications/{id}/reject

## Sự kiện
GET /api/events
GET /api/events/{id}
GET /api/events/mine
GET /api/events/manage
POST /api/events
PUT /api/events/{id}
POST /api/events/{id}/submit
POST /api/events/{id}/approve
POST /api/events/{id}/reject
POST /api/events/{id}/cancel

## Địa điểm
GET /api/venues
POST /api/venues
PUT /api/venues/{id}
POST /api/venue-bookings
POST /api/venue-bookings/{id}/approve
POST /api/venue-bookings/{id}/reject
POST /api/venue-bookings/{id}/cancel

## Vé / đơn / thanh toán
GET /api/events/{eventId}/ticket-types
POST /api/events/{eventId}/ticket-types
PUT /api/ticket-types/{id}
POST /api/orders
GET /api/orders/me
GET /api/orders/{id}
POST /api/orders/{id}/cancel
POST /api/payments
POST /api/payments/webhook
POST /api/payments/{id}/refund
GET /api/tickets/me
GET /api/tickets/{id}
GET /api/tickets/by-code/{code}
POST /api/check-in
POST /api/tickets/{id}/export
POST /api/tickets/{id}/email

## Admin
GET /api/admin/dashboard
