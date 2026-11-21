namespace QLSK.Features.Tickets;

public sealed record TicketDto(int Id, string Label, string EventName, string SessionName, string VenueName,
    string Type, string Zone, string? Seat, string Status, string StatusLabel, string? QrImageUrl,
    IReadOnlyList<string> AllowedActions, string? Code = null);
public sealed record TicketFileDto(byte[] Content, string ContentType, string? Filename);
public sealed record SeatDto(int Id, string Label, int? GroupId, string Status, int? TicketTypeId, string? TypeLabel, decimal? Price);
public sealed record SeatingZoneDto(int Id, string Name, IReadOnlyList<SeatDto> Seats);
public sealed record InventoryDto(string EventName, string SessionName, DateTimeOffset? Start, int? PolicyId, int RemainingQuota,
    IReadOnlyList<SeatingZoneDto> Zones, bool PaidCheckoutAvailable);
public sealed record OrderItemRequest(int TicketTypeId, int SeatId);
public sealed record PlaceOrderRequest(int SessionId, int[]? SeatIds, OrderItemRequest[]? Items,
    string? VoucherCode, int PolicyId, string? IdempotencyKey);
public sealed record OrderItemDto(string Type, string Zone, string? Seat, decimal Price);
public sealed record OrderDto(int Id, string Label, string EventName, string SessionName, DateTimeOffset? CreatedAt,
    decimal Subtotal, decimal Discount, decimal Fee, decimal Total, string Status, string StatusLabel, DateTimeOffset? ExpiresAt,
    IReadOnlyList<OrderItemDto>? Items, IReadOnlyList<string> AllowedActions);
