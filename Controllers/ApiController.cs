using ChauThanhEV.Models;
using ChauThanhEV.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChauThanhEV.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class ApiController : ControllerBase
    {
        private readonly MockDataService _data;

        public ApiController(MockDataService data)
        {
            _data = data;
        }

        // ==========================================
        // STATIONS & CHARGERS
        // ==========================================
        [HttpGet("stations")]
        public IActionResult GetStations()
        {
            var result = _data.Stations.Select(s =>
            {
                var chargers = _data.Chargers.Where(c => c.StationId == s.Id).Select(c => new
                {
                    c.Id,
                    c.Code,
                    c.Name,
                    c.PowerKw,
                    Status = c.Status.ToString(),
                    Connectors = c.Connectors.Select(cn => new
                    {
                        cn.Id,
                        cn.Code,
                        cn.ConnectorType,
                        Status = cn.Status.ToString()
                    })
                }).ToList();

                int totalConnectors = chargers.Sum(c => c.Connectors.Count());
                int availableConnectors = chargers.Sum(c => c.Connectors.Count(cn => cn.Status == ConnectorStatus.Available.ToString()));
                int chargingConnectors = chargers.Sum(c => c.Connectors.Count(cn => cn.Status == ConnectorStatus.Charging.ToString()));

                return new
                {
                    s.Id,
                    s.Code,
                    s.Name,
                    s.Address,
                    s.DefaultElectricityPrice,
                    s.Active,
                    TotalConnectors = totalConnectors,
                    AvailableConnectors = availableConnectors,
                    ChargingConnectors = chargingConnectors,
                    Chargers = chargers
                };
            });

            return Ok(new { success = true, data = result });
        }

        [HttpGet("stations/{id}")]
        public IActionResult GetStationById(int id)
        {
            var s = _data.Stations.FirstOrDefault(x => x.Id == id);
            if (s == null) return NotFound(new { success = false, message = "Không tìm thấy trạm sạc" });

            var chargers = _data.Chargers.Where(c => c.StationId == s.Id).Select(c => new
            {
                c.Id,
                c.Code,
                c.Name,
                c.PowerKw,
                Status = c.Status.ToString(),
                Connectors = c.Connectors.Select(cn => new
                {
                    cn.Id,
                    cn.Code,
                    cn.ConnectorType,
                    Status = cn.Status.ToString()
                })
            }).ToList();

            return Ok(new
            {
                success = true,
                data = new
                {
                    s.Id,
                    s.Code,
                    s.Name,
                    s.Address,
                    s.DefaultElectricityPrice,
                    s.Active,
                    Chargers = chargers
                }
            });
        }

        [HttpGet("connector/lookup")]
        public IActionResult LookupConnector([FromQuery] string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest(new { success = false, message = "Vui lòng cung cấp mã cổng hoặc trụ sạc" });

            code = code.Trim().ToUpperInvariant();

            // Tìm theo mã cổng hoặc mã trụ
            foreach (var charger in _data.Chargers)
            {
                var conn = charger.Connectors.FirstOrDefault(cn => cn.Code.ToUpperInvariant() == code);
                if (conn != null)
                {
                    var station = _data.Stations.FirstOrDefault(s => s.Id == charger.StationId);
                    return Ok(new
                    {
                        success = true,
                        data = new
                        {
                            Station = new { station?.Id, station?.Name, station?.Address, station?.DefaultElectricityPrice },
                            Charger = new { charger.Id, charger.Code, charger.Name, charger.PowerKw, Status = charger.Status.ToString() },
                            Connector = new { conn.Id, conn.Code, conn.ConnectorType, Status = conn.Status.ToString() }
                        }
                    });
                }
            }

            // Nếu tìm theo mã trụ
            var matchCharger = _data.Chargers.FirstOrDefault(c => c.Code.ToUpperInvariant() == code);
            if (matchCharger != null)
            {
                var station = _data.Stations.FirstOrDefault(s => s.Id == matchCharger.StationId);
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        Station = new { station?.Id, station?.Name, station?.Address, station?.DefaultElectricityPrice },
                        Charger = new { matchCharger.Id, matchCharger.Code, matchCharger.Name, matchCharger.PowerKw, Status = matchCharger.Status.ToString() },
                        Connectors = matchCharger.Connectors.Select(cn => new
                        {
                            cn.Id,
                            cn.Code,
                            cn.ConnectorType,
                            Status = cn.Status.ToString()
                        })
                    }
                });
            }

            return NotFound(new { success = false, message = $"Không tìm thấy cổng sạc hoặc trụ sạc có mã {code}" });
        }

        // ==========================================
        // CUSTOMER & AUTH
        // ==========================================
        [HttpGet("customers")]
        public IActionResult GetCustomers()
        {
            var list = _data.Customers.Where(c => c.Status == CustomerStatus.Active).Take(10).Select(c => new
            {
                c.Id,
                c.Code,
                c.FullName,
                c.Phone,
                c.Email,
                c.WalletBalance
            });
            return Ok(new { success = true, data = list });
        }

        [HttpGet("customer/{id}")]
        public IActionResult GetCustomer(int id)
        {
            var c = _data.Customers.FirstOrDefault(x => x.Id == id);
            if (c == null) return NotFound(new { success = false, message = "Không tìm thấy khách hàng" });

            return Ok(new
            {
                success = true,
                data = new
                {
                    c.Id,
                    c.Code,
                    c.FullName,
                    c.Phone,
                    c.Email,
                    c.WalletBalance,
                    Status = c.Status.ToString()
                }
            });
        }

        // ==========================================
        // CHARGING SESSION
        // ==========================================
        public class StartChargingRequest
        {
            public int CustomerId { get; set; }
            public int ConnectorId { get; set; }
            public double TargetKwh { get; set; }
            public decimal TargetAmount { get; set; }
        }

        [HttpPost("charging/start")]
        public IActionResult StartCharging([FromBody] StartChargingRequest req)
        {
            var customer = _data.Customers.FirstOrDefault(c => c.Id == req.CustomerId);
            if (customer == null) return BadRequest(new { success = false, message = "Khách hàng không tồn tại" });
            if (customer.WalletBalance < 50000m)
                return BadRequest(new { success = false, message = "Số dư ví tối thiểu để bắt đầu sạc là 50.000 đ. Vui lòng nạp tiền vào ví." });

            var order = _data.StartUserCharging(req.CustomerId, req.ConnectorId, req.TargetKwh, req.TargetAmount);
            if (order == null)
                return BadRequest(new { success = false, message = "Cổng sạc không khả dụng hoặc đang có xe khác sạc." });

            var charger = _data.Chargers.FirstOrDefault(c => c.Id == order.ChargerId);
            var conn = charger?.Connectors.FirstOrDefault(c => c.Id == order.ConnectorId);
            var station = _data.Stations.FirstOrDefault(s => s.Id == order.StationId);

            return Ok(new
            {
                success = true,
                message = "Đã kích hoạt phiên sạc thành công!",
                data = new
                {
                    OrderId = order.Id,
                    OrderCode = order.Code,
                    order.StartTime,
                    StationName = station?.Name ?? "",
                    ChargerCode = charger?.Code ?? "",
                    ConnectorCode = conn?.Code ?? "",
                    PowerKw = charger?.PowerKw ?? 60.0,
                    PricePerKwh = station?.DefaultElectricityPrice ?? 3800m
                }
            });
        }

        public class StopChargingRequest
        {
            public int OrderId { get; set; }
            public double EnergyKwh { get; set; }
            public decimal Amount { get; set; }
        }

        [HttpPost("charging/stop")]
        public IActionResult StopCharging([FromBody] StopChargingRequest req)
        {
            var order = _data.StopUserCharging(req.OrderId, req.EnergyKwh, req.Amount);
            if (order == null)
                return BadRequest(new { success = false, message = "Không tìm thấy phiên sạc hoặc phiên sạc đã kết thúc." });

            var customer = _data.Customers.FirstOrDefault(c => c.Id == order.CustomerId);
            var charger = _data.Chargers.FirstOrDefault(c => c.Id == order.ChargerId);
            var station = _data.Stations.FirstOrDefault(s => s.Id == order.StationId);

            return Ok(new
            {
                success = true,
                message = "Phiên sạc đã hoàn tất an toàn!",
                data = new
                {
                    OrderId = order.Id,
                    OrderCode = order.Code,
                    order.StartTime,
                    order.EndTime,
                    order.EnergyKwh,
                    order.Amount,
                    RemainingBalance = customer?.WalletBalance ?? 0m,
                    StationName = station?.Name ?? "",
                    ChargerCode = charger?.Code ?? ""
                }
            });
        }

        [HttpGet("charging/active/{customerId}")]
        public IActionResult GetActiveSession(int customerId)
        {
            var order = _data.GetActiveChargingOrder(customerId);
            if (order == null) return Ok(new { success = true, hasActive = false });

            var charger = _data.Chargers.FirstOrDefault(c => c.Id == order.ChargerId);
            var conn = charger?.Connectors.FirstOrDefault(c => c.Id == order.ConnectorId);
            var station = _data.Stations.FirstOrDefault(s => s.Id == order.StationId);

            return Ok(new
            {
                success = true,
                hasActive = true,
                data = new
                {
                    OrderId = order.Id,
                    OrderCode = order.Code,
                    order.StartTime,
                    StationName = station?.Name ?? "",
                    ChargerCode = charger?.Code ?? "",
                    ConnectorCode = conn?.Code ?? "",
                    PowerKw = charger?.PowerKw ?? 60.0,
                    PricePerKwh = station?.DefaultElectricityPrice ?? 3800m
                }
            });
        }

        // ==========================================
        // WALLET TOPUP
        // ==========================================
        public class TopUpRequest
        {
            public int CustomerId { get; set; }
            public decimal Amount { get; set; }
            public string Method { get; set; } = "VietQR";
        }

        [HttpPost("wallet/topup")]
        public IActionResult TopUpWallet([FromBody] TopUpRequest req)
        {
            if (req.Amount <= 0) return BadRequest(new { success = false, message = "Số tiền nạp phải lớn hơn 0" });

            var topup = _data.TopUpUserWallet(req.CustomerId, req.Amount, req.Method);
            if (topup == null) return BadRequest(new { success = false, message = "Nạp tiền không thành công" });

            var customer = _data.Customers.FirstOrDefault(c => c.Id == req.CustomerId);

            return Ok(new
            {
                success = true,
                message = $"Nạp thành công {MockDataService.FormatCurrency(req.Amount)} vào ví!",
                data = new
                {
                    topup.Id,
                    topup.Code,
                    topup.Amount,
                    topup.Method,
                    topup.CreatedAt,
                    NewBalance = customer?.WalletBalance ?? 0m
                }
            });
        }

        // ==========================================
        // REPORT FAULT
        // ==========================================
        public class FaultReportRequest
        {
            public int CustomerId { get; set; }
            public int ChargerId { get; set; }
            public int? ConnectorId { get; set; }
            public string Description { get; set; } = "";
            public int Severity { get; set; } = 1; // 0=Low, 1=Medium, 2=High
        }

        [HttpPost("faults/report")]
        public IActionResult ReportFault([FromBody] FaultReportRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Description))
                return BadRequest(new { success = false, message = "Vui lòng nhập mô tả sự cố" });

            var severity = (FaultSeverity)Math.Clamp(req.Severity, 0, 2);
            var fault = _data.ReportUserFault(req.CustomerId, req.ChargerId, req.ConnectorId, req.Description, severity);
            if (fault == null) return BadRequest(new { success = false, message = "Không tìm thấy trụ sạc" });

            return Ok(new
            {
                success = true,
                message = "Hệ thống đã tiếp nhận phản ánh của quý khách! Đội ngũ vận hành sẽ xử lý ngay lập tức.",
                data = new
                {
                    fault.Id,
                    fault.Code,
                    fault.Description,
                    Severity = fault.Severity.ToString(),
                    Status = fault.Status.ToString(),
                    fault.ReportedAt
                }
            });
        }

        // ==========================================
        // ORDERS HISTORY
        // ==========================================
        [HttpGet("orders/charging/{customerId}")]
        public IActionResult GetCustomerChargingOrders(int customerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var orders = _data.ChargingOrders
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o =>
                {
                    var station = _data.Stations.FirstOrDefault(s => s.Id == o.StationId);
                    var charger = _data.Chargers.FirstOrDefault(c => c.Id == o.ChargerId);
                    var conn = charger?.Connectors.FirstOrDefault(c => c.Id == o.ConnectorId);
                    return new
                    {
                        o.Id,
                        o.Code,
                        StationName = station?.Name ?? "",
                        ChargerCode = charger?.Code ?? "",
                        ConnectorCode = conn?.Code ?? "",
                        o.StartTime,
                        o.EndTime,
                        o.EnergyKwh,
                        o.Amount,
                        o.PaymentMethod,
                        Status = o.Status.ToString()
                    };
                }).ToList();

            return Ok(new { success = true, data = orders });
        }

        [HttpGet("orders/topup/{customerId}")]
        public IActionResult GetCustomerTopUpOrders(int customerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var list = _data.TopUpOrders
                .Where(t => t.CustomerId == customerId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.Id,
                    t.Code,
                    t.Amount,
                    t.Method,
                    t.CreatedAt,
                    Status = t.Status.ToString()
                }).ToList();

            return Ok(new { success = true, data = list });
        }
    }
}
