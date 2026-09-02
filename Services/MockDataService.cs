using System.Globalization;
using ChauThanhEV.Models;

namespace ChauThanhEV.Services
{
    // Toàn bộ hệ thống dùng CHUNG một instance Singleton của lớp này làm "nguồn dữ liệu duy nhất".
    // Dashboard và mọi trang quản lý đều tính toán số liệu từ các danh sách bên dưới,
    // vì vậy khi một trang cập nhật dữ liệu (vd: xử lý lỗi, thêm trụ...), Dashboard sẽ tự phản ánh đúng.
    public class MockDataService
    {
        private static readonly CultureInfo Vi = new("vi-VN");

        // Mốc thời gian "hiện tại" được cố định tại thời điểm khởi động ứng dụng,
        // để các số liệu "hôm nay/tháng này" ổn định trong suốt phiên chạy demo.
        public DateTime Now { get; } = DateTime.Now;

        public List<Station> Stations { get; } = new();
        public List<Charger> Chargers { get; } = new();
        public List<Customer> Customers { get; } = new();
        public List<ChargingOrder> ChargingOrders { get; } = new();
        public List<TopUpOrder> TopUpOrders { get; } = new();
        public List<FaultRecord> Faults { get; } = new();
        public List<AbnormalOrder> AbnormalOrders { get; } = new();
        public List<TopUpPackage> TopUpPackages { get; } = new();
        public List<RfidCard> Cards { get; } = new();
        public List<Reservation> Reservations { get; } = new();
        public List<FinancialTransaction> Transactions { get; } = new();

        private int _nextChargerId = 1;
        private int _nextConnectorId = 1;
        private int _nextCustomerId = 1;
        private int _nextChargingOrderId = 1;
        private int _nextTopUpOrderId = 1;
        private int _nextFaultId = 1;
        private int _nextAbnormalId = 1;

        public MockDataService()
        {
            Seed();
        }

        // ============================================================
        // SEED DATA
        // ============================================================
        private void Seed()
        {
            var rnd = new Random(20260902);

            // ---- Stations ----
            var stationInfos = new (string Code, string Name, string Address, bool Active)[]
            {
                ("TTC-CT-01", "TTC Châu Thành 1", "Km205+092, cao tốc Vĩnh Hảo – Phan Thiết, bên trái tuyến (mock)", true),
                ("TTC-CT-02", "TTC Châu Thành 2", "Km205+092, cao tốc Vĩnh Hảo – Phan Thiết, bên phải tuyến (mock)", true),
                ("REF-PD-KM47", "Trạm Km47+500", "Cao tốc Phan Thiết – Dầu Giây, trạm đối tác tham chiếu (mock)", true),
                ("PROP-KM31-32", "Trạm Km31–32", "Cao tốc Phan Thiết – Dầu Giây, vị trí đề xuất (proposed)", false),
                ("PROP-KM41-100", "Km41+100", "Cao tốc TP.HCM – Long Thành – Dầu Giây, mở rộng mạng lưới (proposed)", false),
            };
            for (int i = 0; i < stationInfos.Length; i++)
            {
                Stations.Add(new Station
                {
                    Id = i + 1,
                    Code = stationInfos[i].Code,
                    Name = stationInfos[i].Name,
                    Address = stationInfos[i].Address,
                    Active = stationInfos[i].Active,
                    DefaultElectricityPrice = 3800m
                });
            }

            // ---- Chargers + Connectors ----
            // Cấu hình mock theo phương án: mỗi trụ DC có đúng 2 cổng CCS2.
            var chargerDefinitions = new (int StationId, string Code, string Name, double PowerKw, ChargerStatus Status, ConnectorStatus A, ConnectorStatus B)[]
            {
                (1, "TTC1-DC-160-01", "DC Fast Charger 160 kW - Trụ 01", 160, ChargerStatus.Online, ConnectorStatus.Available, ConnectorStatus.Charging),
                (1, "TTC1-DC-240-01", "DC Fast Charger 240 kW - Trụ 02", 240, ChargerStatus.Online, ConnectorStatus.Available, ConnectorStatus.Available),
                (1, "TTC1-DC-160-02", "DC Fast Charger 160 kW - Trụ 03", 160, ChargerStatus.Maintenance, ConnectorStatus.Maintenance, ConnectorStatus.Maintenance),
                (2, "TTC2-DC-240-01", "DC Fast Charger 240 kW - Trụ 01", 240, ChargerStatus.Online, ConnectorStatus.Charging, ConnectorStatus.Available),
                (2, "TTC2-DC-160-01", "DC Fast Charger 160 kW - Trụ 02", 160, ChargerStatus.Offline, ConnectorStatus.Fault, ConnectorStatus.Fault),
                (2, "TTC2-DC-240-02", "DC Fast Charger 240 kW - Trụ 03", 240, ChargerStatus.Online, ConnectorStatus.Available, ConnectorStatus.Fault),
                (3, "REF47-DC-160-01", "DC Fast Charger 160 kW - Trụ 01", 160, ChargerStatus.Online, ConnectorStatus.Available, ConnectorStatus.Available),
                (3, "REF47-DC-240-01", "DC Fast Charger 240 kW - Trụ 02", 240, ChargerStatus.Online, ConnectorStatus.Charging, ConnectorStatus.Available),
            };
            foreach (var definition in chargerDefinitions)
            {
                var charger = new Charger
                {
                    Id = _nextChargerId++,
                    Code = definition.Code,
                    Name = definition.Name,
                    StationId = definition.StationId,
                    PowerKw = definition.PowerKw,
                    Status = definition.Status,
                    InstallDate = Now.AddDays(-180 - _nextChargerId * 9)
                };
                foreach (var connector in new[] { ("A", definition.A), ("B", definition.B) })
                {
                    charger.Connectors.Add(new Connector
                    {
                        Id = _nextConnectorId++,
                        Code = $"{charger.Code}-{connector.Item1}",
                        ChargerId = charger.Id,
                        ConnectorType = "CCS2",
                        Status = connector.Item2
                    });
                }
                Chargers.Add(charger);
            }

            // ---- Faults: gắn với trụ offline + cổng lỗi hiện tại, cộng thêm lịch sử đã xử lý ----
            var faultConnector = Chargers.SelectMany(c => c.Connectors).First(cn => cn.Status == ConnectorStatus.Fault);
            var faultCharger = Chargers.First(c => c.Connectors.Any(cn => cn.Id == faultConnector.Id));
            Faults.Add(new FaultRecord
            {
                Id = _nextFaultId++,
                Code = $"LOI-{_nextFaultId - 1:0000}",
                ChargerId = faultCharger.Id,
                ConnectorId = faultConnector.Id,
                Description = "Cổng sạc báo lỗi giao tiếp với xe, không thể cấp điện.",
                Severity = FaultSeverity.High,
                Status = FaultStatus.Processing,
                ReportedAt = Now.AddHours(-6)
            });

            var offlineCharger = Chargers.First(c => c.Status == ChargerStatus.Offline);
            Faults.Add(new FaultRecord { Id = _nextFaultId++, Code = $"LOI-{_nextFaultId - 1:0000}", ChargerId = offlineCharger.Id, Description = "Mất kết nối OCPP, trụ không phản hồi trung tâm điều khiển.", Severity = FaultSeverity.High, Status = FaultStatus.Processing, ReportedAt = Now.AddHours(-9) });
            var maintenanceCharger = Chargers.First(c => c.Status == ChargerStatus.Maintenance);
            Faults.Add(new FaultRecord { Id = _nextFaultId++, Code = $"LOI-{_nextFaultId - 1:0000}", ChargerId = maintenanceCharger.Id, Description = "Lịch bảo trì định kỳ: kiểm tra cáp CCS2 và hệ thống làm mát.", Severity = FaultSeverity.Medium, Status = FaultStatus.New, ReportedAt = Now.AddHours(-3) });

            // Lịch sử lỗi đã xử lý (làm phong phú danh sách "Lịch sử lỗi")
            var sampleDescriptions = new[]
            {
                "Cổng sạc dừng đột ngột giữa phiên sạc.",
                "Màn hình hiển thị trụ bị treo, phải khởi động lại.",
                "Relay đóng ngắt tiếp xúc kém, cần thay thế.",
                "Lỗi cảm biến nhiệt độ, trụ tự giảm công suất.",
                "Cáp sạc bị nứt vỏ, cần kiểm tra an toàn.",
                "Trụ báo lỗi quá dòng, đã reset lại hệ thống."
            };
            for (int i = 0; i < 6; i++)
            {
                var ch = Chargers[rnd.Next(Chargers.Count)];
                var conn = ch.Connectors[rnd.Next(ch.Connectors.Count)];
                var reportedAt = Now.AddDays(-rnd.Next(3, 29)).AddHours(-rnd.Next(0, 23));
                Faults.Add(new FaultRecord
                {
                    Id = _nextFaultId++,
                    Code = $"LOI-{_nextFaultId - 1:0000}",
                    ChargerId = ch.Id,
                    ConnectorId = conn.Id,
                    Description = sampleDescriptions[rnd.Next(sampleDescriptions.Length)],
                    Severity = (FaultSeverity)rnd.Next(0, 3),
                    Status = FaultStatus.Resolved,
                    ReportedAt = reportedAt,
                    ResolvedAt = reportedAt.AddHours(rnd.Next(1, 20))
                });
            }

            // ---- Customer & other management ----
            var customerProfiles = new (string Name, string Phone, string Email, decimal Balance, CustomerStatus Status)[]
            {
                ("Nguyễn Minh Anh", "0903120456", "minhanh.mock@chauthanhev.vn", 1250000m, CustomerStatus.Active),
                ("Trần Gia Huy", "0918472301", "giahuy.mock@chauthanhev.vn", 680000m, CustomerStatus.Active),
                ("Lê Hoàng Nam", "0985123789", "hoangnam.mock@chauthanhev.vn", 2150000m, CustomerStatus.Active),
                ("Phạm Ngọc Mai", "0934781265", "ngocmai.mock@chauthanhev.vn", 420000m, CustomerStatus.Active),
                ("Võ Thanh Tùng", "0976312048", "thanhtung.mock@chauthanhev.vn", 950000m, CustomerStatus.Active),
                ("Đỗ Khánh Linh", "0908765123", "khanhlinh.mock@chauthanhev.vn", 300000m, CustomerStatus.Locked),
                ("Bùi Quốc Việt", "0942018367", "quocviet.mock@chauthanhev.vn", 1780000m, CustomerStatus.Active),
                ("Ngô Thùy Dương", "0967452108", "thuyduong.mock@chauthanhev.vn", 560000m, CustomerStatus.Active),
                ("Huỳnh Đức Long", "0912356780", "duclong.mock@chauthanhev.vn", 830000m, CustomerStatus.Active),
                ("Lý Hải Yến", "0987345612", "haiyen.mock@chauthanhev.vn", 1120000m, CustomerStatus.Active),
                ("Dương Nhật Quang", "0938120674", "nhatquang.mock@chauthanhev.vn", 250000m, CustomerStatus.Active),
                ("Mai Thị Bích Ngọc", "0904567812", "bichngoc.mock@chauthanhev.vn", 1460000m, CustomerStatus.Active)
            };
            for (int i = 0; i < customerProfiles.Length; i++)
            {
                var profile = customerProfiles[i];
                Customers.Add(new Customer
                {
                    Id = _nextCustomerId,
                    Code = $"KH-CT-{_nextCustomerId++:0000}",
                    FullName = profile.Name,
                    Phone = profile.Phone,
                    Email = profile.Email,
                    WalletBalance = profile.Balance,
                    Status = profile.Status,
                    RegisteredAt = Now.AddDays(-45 - i * 17)
                });
            }

            // ---- Charging orders & Top-up orders: sinh theo 40 ngày gần nhất ----
            string[] paymentMethods = { "Ví điện tử", "Thẻ ngân hàng", "Chuyển khoản" };
            string[] topupMethods = { "Chuyển khoản ngân hàng", "Ví Momo", "Thẻ tín dụng" };

            for (int daysAgo = 39; daysAgo >= 0; daysAgo--)
            {
                var day = Now.Date.AddDays(-daysAgo);
                bool isToday = daysAgo == 0;

                int chargingCount = Math.Max(6, (int)Math.Round(58 - daysAgo * 1.15 + rnd.Next(-4, 5)));
                for (int j = 0; j < chargingCount; j++)
                {
                    var customer = Customers[rnd.Next(Customers.Count)];
                    var operationalChargers = Chargers.Where(c => c.Status == ChargerStatus.Online).ToList();
                    var charger = operationalChargers[rnd.Next(operationalChargers.Count)];
                    var connector = charger.Connectors[rnd.Next(charger.Connectors.Count)];
                    var hour = WeightedHour(rnd);
                    var start = day.AddHours(hour).AddMinutes(rnd.Next(0, 59));
                    if (start > Now) start = Now.AddMinutes(-rnd.Next(5, 40));

                    var durationMin = rnd.Next(18, 95);
                    var energy = Math.Round(durationMin * (rnd.Next(18, 34) / 10.0), 1);
                    var amount = Math.Round((decimal)energy * 3800m, 0);

                    ChargingOrderStatus status;
                    DateTime? end = start.AddMinutes(durationMin);
                    double roll = rnd.NextDouble();
                    if (isToday && j < 3)
                    {
                        status = ChargingOrderStatus.Charging;
                        end = null;
                    }
                    else if (roll < 0.04)
                    {
                        status = ChargingOrderStatus.Cancelled;
                        energy = 0; amount = 0;
                        end = start.AddMinutes(2);
                    }
                    else if (roll < 0.07)
                    {
                        status = ChargingOrderStatus.Failed;
                        energy = Math.Round(energy * 0.15, 1);
                        amount = Math.Round((decimal)energy * 3800m, 0);
                    }
                    else if (roll < 0.09)
                    {
                        status = ChargingOrderStatus.Abnormal;
                    }
                    else
                    {
                        status = ChargingOrderStatus.Completed;
                    }

                    var id = _nextChargingOrderId++;
                    ChargingOrders.Add(new ChargingOrder
                    {
                        Id = id,
                        Code = $"SC-{id:000000}",
                        CustomerId = customer.Id,
                        ChargerId = charger.Id,
                        ConnectorId = connector.Id,
                        StationId = charger.StationId,
                        StartTime = start,
                        EndTime = end,
                        EnergyKwh = energy,
                        Amount = amount,
                        PaymentMethod = paymentMethods[rnd.Next(paymentMethods.Length)],
                        Status = status
                    });
                }

                int topupCount = Math.Max(2, (int)Math.Round(14 - daysAgo * 0.28 + rnd.Next(-2, 3)));
                decimal[] topupAmounts = { 50000m, 100000m, 200000m, 300000m, 500000m, 1000000m };
                for (int j = 0; j < topupCount; j++)
                {
                    var customer = Customers[rnd.Next(Customers.Count)];
                    var hour = WeightedHour(rnd);
                    var created = day.AddHours(hour).AddMinutes(rnd.Next(0, 59));
                    if (created > Now) created = Now.AddMinutes(-rnd.Next(2, 30));

                    TopUpStatus status = TopUpStatus.Success;
                    double roll = rnd.NextDouble();
                    if (isToday && created > Now.AddMinutes(-15)) status = TopUpStatus.Processing;
                    else if (roll < 0.05) status = TopUpStatus.Failed;

                    var id = _nextTopUpOrderId++;
                    TopUpOrders.Add(new TopUpOrder
                    {
                        Id = id,
                        Code = $"NT-{id:000000}",
                        CustomerId = customer.Id,
                        Amount = topupAmounts[rnd.Next(topupAmounts.Length)],
                        Method = topupMethods[rnd.Next(topupMethods.Length)],
                        CreatedAt = created,
                        Status = status
                    });
                }
            }

            // ---- Abnormal orders: chọn ngẫu nhiên một số đơn gần đây để gắn cờ bất thường ----
            var abnormalTypes = Enum.GetValues<AbnormalOrderType>();
            var recentCharging = ChargingOrders.Where(o => o.StartTime >= Now.AddDays(-10) && o.Status != ChargingOrderStatus.Cancelled)
                                                .OrderBy(_ => rnd.Next()).Take(6).ToList();
            var recentTopup = TopUpOrders.Where(o => o.CreatedAt >= Now.AddDays(-10))
                                          .OrderBy(_ => rnd.Next()).Take(5).ToList();

            foreach (var order in recentCharging)
            {
                var type = abnormalTypes[rnd.Next(abnormalTypes.Length)];
                bool resolved = rnd.NextDouble() < 0.4;
                var id = _nextAbnormalId++;
                AbnormalOrders.Add(new AbnormalOrder
                {
                    Id = id,
                    Code = $"BT-{id:0000}",
                    OrderType = SourceOrderType.Charging,
                    OrderCode = order.Code,
                    CustomerId = order.CustomerId,
                    Amount = order.Amount,
                    Type = type,
                    Note = DescribeAbnormal(type, order.Code),
                    Status = resolved ? AbnormalOrderStatus.Resolved : AbnormalOrderStatus.Pending,
                    DetectedAt = order.StartTime.AddMinutes(rnd.Next(5, 60)),
                    ResolutionNote = resolved ? "Đã kiểm tra và xử lý ổn thỏa với khách hàng." : null
                });
            }
            foreach (var order in recentTopup)
            {
                var type = rnd.NextDouble() < 0.5 ? AbnormalOrderType.WrongAmount : AbnormalOrderType.PaymentFailed;
                bool resolved = rnd.NextDouble() < 0.4;
                var id = _nextAbnormalId++;
                AbnormalOrders.Add(new AbnormalOrder
                {
                    Id = id,
                    Code = $"BT-{id:0000}",
                    OrderType = SourceOrderType.TopUp,
                    OrderCode = order.Code,
                    CustomerId = order.CustomerId,
                    Amount = order.Amount,
                    Type = type,
                    Note = DescribeAbnormal(type, order.Code),
                    Status = resolved ? AbnormalOrderStatus.Resolved : AbnormalOrderStatus.Pending,
                    DetectedAt = order.CreatedAt.AddMinutes(rnd.Next(5, 60)),
                    ResolutionNote = resolved ? "Đã hoàn tất đối soát với ngân hàng/ví điện tử." : null
                });
            }

            // ---- Top-up packages ----
            var packages = new[]
            {
                ("EVP Khởi hành", 100000m, 5000m),
                ("EVP Tiết kiệm", 300000m, 20000m),
                ("EVP Cao tốc", 500000m, 40000m),
                ("EVP Đồng hành", 1000000m, 90000m),
                ("EVP Đội xe", 2000000m, 200000m)
            };
            foreach (var pkg in packages)
            {
                TopUpPackages.Add(new TopUpPackage
                {
                    Id = TopUpPackages.Count + 1,
                    Name = pkg.Item1,
                    Amount = pkg.Item2,
                    Bonus = pkg.Item3,
                    Active = true,
                    CreatedAt = Now.AddDays(-rnd.Next(10, 200))
                });
            }

            // ---- RFID cards ----
            for (int i = 0; i < 8; i++)
            {
                var customer = Customers[i * 18 % Customers.Count];
                Cards.Add(new RfidCard
                {
                    Id = i + 1,
                    CardId = $"CTEVP-RFID-{(i + 1):0000}",
                    UserId = customer.Id,
                    UserName = customer.FullName,
                    Status = i % 2 == 0 ? CardStatus.Active : CardStatus.Inactive,
                    CreatedDate = Now.AddDays(-(i + 5) * 14),
                    ExpiryDate = Now.AddMonths(i + 6)
                });
            }

            // ---- Reservations ----
            for (int i = 0; i < 8; i++)
            {
                var customer = Customers[i % Customers.Count];
                var charger = Chargers[i % Chargers.Count];
                var start = Now.AddDays(i % 4).AddHours(8 + (i * 2 % 10));
                Reservations.Add(new Reservation
                {
                    Id = i + 1,
                    ReservationId = $"RES-{(i + 1):0000}",
                    UserId = customer.Id,
                    UserName = customer.FullName,
                    ChargerId = charger.Id,
                    ChargerCode = charger.Code,
                    StartTime = start,
                    EndTime = start.AddHours(2 + (i % 3)),
                    Status = i % 3 == 0 ? ReservationStatus.Active : (i % 3 == 1 ? ReservationStatus.Expired : ReservationStatus.Cancelled)
                });
            }

            // ---- Financial transactions: lấy trực tiếp từ đơn nạp/đơn sạc đã seed ----
            decimal runningBalance = 28500000m;
            var financialEvents = TopUpOrders.Where(o => o.Status == TopUpStatus.Success)
                .Select(o => (CreatedAt: o.CreatedAt, Type: "Nạp tiền EVP", Amount: o.Amount, Note: $"Đối soát {o.Code}"))
                .Concat(ChargingOrders.Where(o => o.Status == ChargingOrderStatus.Completed)
                    .Select(o => (CreatedAt: o.StartTime, Type: "Doanh thu sạc", Amount: o.Amount, Note: $"Đối soát {o.Code}")))
                .OrderBy(e => e.CreatedAt)
                .Take(18)
                .ToList();
            foreach (var item in financialEvents)
            {
                runningBalance += item.Type == "Doanh thu sạc" ? item.Amount : item.Amount;
                Transactions.Add(new FinancialTransaction
                {
                    Id = Transactions.Count + 1,
                    TransactionId = $"CT-TXN-{Transactions.Count + 1:0000}",
                    TransactionTime = item.CreatedAt,
                    TransactionType = item.Type,
                    Amount = item.Amount,
                    BalanceAfter = runningBalance,
                    Note = item.Note
                });
            }
            Transactions.Add(new FinancialTransaction
            {
                Id = Transactions.Count + 1,
                TransactionId = $"CT-TXN-{Transactions.Count + 1:0000}",
                TransactionTime = Now.AddHours(-4),
                TransactionType = "Rút tiền",
                Amount = 5000000m,
                BalanceAfter = runningBalance - 5000000m,
                Note = "Rút doanh thu mock về tài khoản đối soát"
            });
        }

        private static int WeightedHour(Random rnd)
        {
            // Ưu tiên giờ cao điểm 6h-22h giống hành vi sạc thực tế
            int[] hours = { 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22 };
            return hours[rnd.Next(hours.Length)];
        }

        private static string DescribeAbnormal(AbnormalOrderType type, string orderCode) => type switch
        {
            AbnormalOrderType.TransactionStuck => $"Giao dịch {orderCode} bị treo, chưa ghi nhận trạng thái hoàn tất.",
            AbnormalOrderType.WrongAmount => $"Số tiền ghi nhận của {orderCode} không khớp với hệ thống thanh toán.",
            AbnormalOrderType.PaymentFailed => $"Đã trừ tiền nhưng giao dịch {orderCode} báo thất bại.",
            AbnormalOrderType.DuplicateCharge => $"Khách hàng phản ánh bị trừ tiền hai lần cho {orderCode}.",
            _ => $"Giao dịch {orderCode} có dấu hiệu bất thường."
        };

        // ============================================================
        // FORMAT HELPERS
        // ============================================================
        public static string FormatCurrency(decimal v) => v.ToString("N0", Vi) + " đ";
        public static string FormatNumber(long v) => v.ToString("N0", Vi);
        public static string FormatNumber(int v) => v.ToString("N0", Vi);
        public static string FormatEnergy(double kwh) => kwh.ToString("N1", Vi) + " kWh";

        public static decimal PercentChange(decimal current, decimal previous)
        {
            if (previous == 0) return current == 0 ? 0m : 100m;
            return Math.Round((current - previous) / previous * 100m, 2);
        }

        // ============================================================
        // DASHBOARD
        // ============================================================
        public List<Connector> GetAllConnectors() => Chargers.SelectMany(c => c.Connectors).ToList();

        public DashboardViewModel GetDashboard()
        {
            var connectors = GetAllConnectors();
            int totalChargers = Chargers.Count;
            int offlineChargers = Chargers.Count(c => c.Status == ChargerStatus.Offline);
            int totalConnectors = connectors.Count;
            int available = connectors.Count(c => c.Status == ConnectorStatus.Available);
            int charging = connectors.Count(c => c.Status == ConnectorStatus.Charging);
            int fault = connectors.Count(c => c.Status == ConnectorStatus.Fault);

            decimal offlinePct = totalChargers == 0 ? 0 : Math.Round(offlineChargers * 100m / totalChargers, 2);
            decimal availablePct = totalConnectors == 0 ? 0 : Math.Round(available * 100m / totalConnectors, 2);
            decimal chargingPct = totalConnectors == 0 ? 0 : Math.Round(charging * 100m / totalConnectors, 2);
            decimal faultPct = totalConnectors == 0 ? 0 : Math.Round(fault * 100m / totalConnectors, 2);

            var model = new DashboardViewModel
            {
                AvailableConnectors = available,
                ChargingConnectors = charging,
                FaultConnectors = fault,
                StationStats = new List<StatCard>
                {
                    new() { IconKey = "station", IconBg = "blue",  Title = "Tổng số trụ sạc",       Value = FormatNumber(totalChargers), Unit = "Trụ" },
                    new() { IconKey = "offline", IconBg = "red",   Title = "Số trụ sạc ngoại tuyến", Value = FormatNumber(offlineChargers), Unit = $"Trụ ({offlinePct.ToString("0.00", Vi)}%)" },
                    new() { IconKey = "plug",    IconBg = "green", Title = "Số cổng sạc sẵn sàng",   Value = FormatNumber(available), Unit = $"Cổng ({availablePct.ToString("0.00", Vi)}%)" },
                    new() { IconKey = "bolt",    IconBg = "blue",  Title = "Số cổng sạc đang sạc",   Value = FormatNumber(charging), Unit = $"Cổng ({chargingPct.ToString("0.00", Vi)}%)" },
                    new() { IconKey = "warning", IconBg = "red",   Title = "Số cổng sạc gặp lỗi",    Value = FormatNumber(fault), Unit = $"Cổng ({faultPct.ToString("0.00", Vi)}%)" },
                }
            };

            var todayStart = Now.Date;
            var todayEnd = todayStart.AddDays(1);
            var yesterdayStart = todayStart.AddDays(-1);
            var monthStart = new DateTime(Now.Year, Now.Month, 1);
            var monthEnd = monthStart.AddMonths(1);
            var prevMonthStart = monthStart.AddMonths(-1);

            var (todayRevenue, todayGmv, todayEnergy, todayOrders, todayUsers) = Aggregate(todayStart, todayEnd);
            var (ydRevenue, ydGmv, ydEnergy, ydOrders, ydUsers) = Aggregate(yesterdayStart, todayStart);
            var (monthRevenue, monthGmv, monthEnergy, monthOrders, monthUsers) = Aggregate(monthStart, monthEnd);
            var (pmRevenue, pmGmv, pmEnergy, pmOrders, pmUsers) = Aggregate(prevMonthStart, monthStart);

            model.TodayGmvValue = FormatCurrency(todayGmv);
            model.TodayGmvCompare = $"Hôm qua: {FormatCurrency(ydGmv)}";
            model.TodayGmvChange = PercentChange(todayGmv, ydGmv);
            model.TodayOverview = new List<OverviewCard>
            {
                new() { IconKey = "wallet", IconBg = "green",  Title = "Tổng thu nhập",          Value = FormatCurrency(todayRevenue), CompareLabel = $"Hôm qua: {FormatCurrency(ydRevenue)}", ChangePercent = PercentChange(todayRevenue, ydRevenue) },
                new() { IconKey = "doc",    IconBg = "blue",   Title = "Số đơn hàng",            Value = FormatNumber(todayOrders),     CompareLabel = $"Hôm qua: {FormatNumber(ydOrders)}",     ChangePercent = PercentChange(todayOrders, ydOrders) },
                new() { IconKey = "bolt2",  IconBg = "orange", Title = "Tổng điện năng tiêu thụ", Value = FormatEnergy(todayEnergy),     CompareLabel = $"Hôm qua: {FormatEnergy(ydEnergy)}",     ChangePercent = PercentChange((decimal)todayEnergy, (decimal)ydEnergy) },
                new() { IconKey = "users",  IconBg = "purple", Title = "Người dùng hoạt động",   Value = FormatNumber(todayUsers),      CompareLabel = $"Hôm qua: {FormatNumber(ydUsers)}",      ChangePercent = PercentChange(todayUsers, ydUsers) },
            };

            model.MonthGmvValue = FormatCurrency(monthGmv);
            model.MonthGmvCompare = $"Tháng trước: {FormatCurrency(pmGmv)}";
            model.MonthGmvChange = PercentChange(monthGmv, pmGmv);
            model.MonthOverview = new List<OverviewCard>
            {
                new() { IconKey = "wallet", IconBg = "green",  Title = "Tổng thu nhập",          Value = FormatCurrency(monthRevenue), CompareLabel = $"Tháng trước: {FormatCurrency(pmRevenue)}", ChangePercent = PercentChange(monthRevenue, pmRevenue) },
                new() { IconKey = "doc",    IconBg = "blue",   Title = "Số đơn hàng",            Value = FormatNumber(monthOrders),     CompareLabel = $"Tháng trước: {FormatNumber(pmOrders)}",     ChangePercent = PercentChange(monthOrders, pmOrders) },
                new() { IconKey = "bolt2",  IconBg = "orange", Title = "Tổng điện năng tiêu thụ", Value = FormatEnergy(monthEnergy),     CompareLabel = $"Tháng trước: {FormatEnergy(pmEnergy)}",     ChangePercent = PercentChange((decimal)monthEnergy, (decimal)pmEnergy) },
                new() { IconKey = "users",  IconBg = "purple", Title = "Người dùng hoạt động",   Value = FormatNumber(monthUsers),      CompareLabel = $"Tháng trước: {FormatNumber(pmUsers)}",      ChangePercent = PercentChange(monthUsers, pmUsers) },
            };

            model.RevenueToday = BuildRevenueSeries(DashboardRange.Today);
            model.Revenue7Days = BuildRevenueSeries(DashboardRange.Last7Days);
            model.Revenue30Days = BuildRevenueSeries(DashboardRange.Last30Days);
            model.ActiveUsersToday = BuildActiveUserSeries(DashboardRange.Today);
            model.ActiveUsers7Days = BuildActiveUserSeries(DashboardRange.Last7Days);
            model.ActiveUsers30Days = BuildActiveUserSeries(DashboardRange.Last30Days);

            return model;
        }

        private (decimal revenue, decimal gmv, double energy, int orders, int users) Aggregate(DateTime start, DateTime end)
        {
            var charging = ChargingOrders.Where(o => o.StartTime >= start && o.StartTime < end).ToList();
            var topups = TopUpOrders.Where(o => o.CreatedAt >= start && o.CreatedAt < end).ToList();

            decimal revenue = charging.Where(o => o.Status == ChargingOrderStatus.Completed).Sum(o => o.Amount);
            decimal topupSum = topups.Where(o => o.Status == TopUpStatus.Success).Sum(o => o.Amount);
            double energy = charging.Sum(o => o.EnergyKwh);
            int orders = charging.Count + topups.Count;
            int users = charging.Select(o => o.CustomerId).Concat(topups.Select(o => o.CustomerId)).Distinct().Count();

            return (revenue, revenue + topupSum, energy, orders, users);
        }

        private ChartSeriesData BuildRevenueSeries(DashboardRange range)
        {
            var data = new ChartSeriesData();
            if (range == DashboardRange.Today)
            {
                for (int h = 0; h <= 24; h += 2)
                {
                    var from = Now.Date.AddHours(h);
                    var to = h == 24 ? Now.Date.AddDays(1) : Now.Date.AddHours(h + 2);
                    decimal sum = ChargingOrders.Where(o => o.StartTime >= from && o.StartTime < to && o.Status == ChargingOrderStatus.Completed).Sum(o => o.Amount);
                    data.Labels.Add(h == 24 ? "24:00" : $"{h:00}:00");
                    data.Values.Add((double)sum / 1_000_000.0); // triệu đồng
                }
            }
            else
            {
                int days = range == DashboardRange.Last7Days ? 7 : 30;
                for (int i = days - 1; i >= 0; i--)
                {
                    var day = Now.Date.AddDays(-i);
                    var next = day.AddDays(1);
                    decimal sum = ChargingOrders.Where(o => o.StartTime >= day && o.StartTime < next && o.Status == ChargingOrderStatus.Completed).Sum(o => o.Amount);
                    data.Labels.Add(day.ToString("dd/MM"));
                    data.Values.Add((double)sum / 1_000_000.0);
                }
            }
            return data;
        }

        private ChartSeriesData BuildActiveUserSeries(DashboardRange range)
        {
            var data = new ChartSeriesData();
            if (range == DashboardRange.Today)
            {
                for (int h = 0; h <= 24; h += 2)
                {
                    var from = Now.Date.AddHours(h);
                    var to = h == 24 ? Now.Date.AddDays(1) : Now.Date.AddHours(h + 2);
                    var users = ChargingOrders.Where(o => o.StartTime >= from && o.StartTime < to).Select(o => o.CustomerId)
                        .Concat(TopUpOrders.Where(o => o.CreatedAt >= from && o.CreatedAt < to).Select(o => o.CustomerId))
                        .Distinct().Count();
                    data.Labels.Add(h == 24 ? "24:00" : $"{h:00}:00");
                    data.Values.Add(users);
                }
            }
            else
            {
                int days = range == DashboardRange.Last7Days ? 7 : 30;
                for (int i = days - 1; i >= 0; i--)
                {
                    var day = Now.Date.AddDays(-i);
                    var next = day.AddDays(1);
                    var users = ChargingOrders.Where(o => o.StartTime >= day && o.StartTime < next).Select(o => o.CustomerId)
                        .Concat(TopUpOrders.Where(o => o.CreatedAt >= day && o.CreatedAt < next).Select(o => o.CustomerId))
                        .Distinct().Count();
                    data.Labels.Add(day.ToString("dd/MM"));
                    data.Values.Add(users);
                }
            }
            return data;
        }

        // ============================================================
        // CHARGERS
        // ============================================================
        private ChargerRow ToRow(Charger c)
        {
            var station = Stations.FirstOrDefault(s => s.Id == c.StationId);
            return new ChargerRow
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                StationId = c.StationId,
                StationName = station?.Name ?? "",
                PowerKw = c.PowerKw,
                Status = c.Status,
                HasFault = c.Connectors.Any(cn => cn.Status == ConnectorStatus.Fault),
                ConnectorCount = c.Connectors.Count,
                AvailableCount = c.Connectors.Count(cn => cn.Status == ConnectorStatus.Available),
                ChargingCount = c.Connectors.Count(cn => cn.Status == ConnectorStatus.Charging),
                FaultCount = c.Connectors.Count(cn => cn.Status == ConnectorStatus.Fault),
                InstallDate = c.InstallDate,
                Connectors = c.Connectors
            };
        }

        public ChargerListViewModel SearchChargers(string? keyword, string? statusFilter, int page, int pageSize = 10)
        {
            var query = Chargers.Select(ToRow).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim().ToLower();
                query = query.Where(r => r.Code.ToLower().Contains(k) || r.Name.ToLower().Contains(k) || r.StationName.ToLower().Contains(k));
            }
            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "all")
            {
                var f = statusFilter;
                query = query.Where(r => string.Equals(r.FilterStatus, f, StringComparison.OrdinalIgnoreCase));
            }

            var ordered = query.OrderBy(r => r.Code).ToList();
            return new ChargerListViewModel
            {
                Keyword = keyword,
                StatusFilter = statusFilter,
                Stations = Stations,
                Result = PagedResult<ChargerRow>.Create(ordered, page, pageSize)
            };
        }

        public Charger? GetChargerById(int id) => Chargers.FirstOrDefault(c => c.Id == id);

        public void AddCharger(ChargerFormModel form)
        {
            var id = _nextChargerId++;
            var code = string.IsNullOrWhiteSpace(form.Code) ? $"TS-{id:000}" : form.Code;
            var charger = new Charger
            {
                Id = id,
                Code = code,
                Name = form.Name,
                StationId = form.StationId,
                PowerKw = form.PowerKw,
                Status = form.Status,
                InstallDate = Now
            };
            var letters = new[] { "A", "B", "C", "D" };
            int count = Math.Clamp(form.ConnectorCount, 1, 4);
            for (int i = 0; i < count; i++)
            {
                charger.Connectors.Add(new Connector
                {
                    Id = _nextConnectorId++,
                    Code = $"{code}-{letters[i]}",
                    ChargerId = id,
                    ConnectorType = i == 0 ? "CCS2" : "AC",
                    Status = ConnectorStatus.Available
                });
            }
            Chargers.Add(charger);
        }

        public bool UpdateCharger(ChargerFormModel form)
        {
            var charger = GetChargerById(form.Id);
            if (charger == null) return false;

            charger.Name = form.Name;
            charger.StationId = form.StationId;
            charger.PowerKw = form.PowerKw;
            charger.Status = form.Status;

            int target = Math.Clamp(form.ConnectorCount, 1, 4);
            var letters = new[] { "A", "B", "C", "D" };
            if (target > charger.Connectors.Count)
            {
                for (int i = charger.Connectors.Count; i < target; i++)
                {
                    charger.Connectors.Add(new Connector
                    {
                        Id = _nextConnectorId++,
                        Code = $"{charger.Code}-{letters[i]}",
                        ChargerId = charger.Id,
                        ConnectorType = "AC",
                        Status = ConnectorStatus.Available
                    });
                }
            }
            else if (target < charger.Connectors.Count)
            {
                charger.Connectors.RemoveRange(target, charger.Connectors.Count - target);
            }
            return true;
        }

        public bool DeleteCharger(int id)
        {
            var charger = GetChargerById(id);
            if (charger == null) return false;
            Chargers.Remove(charger);
            return true;
        }

        public bool ReactivateCharger(int id)
        {
            var charger = GetChargerById(id);
            if (charger == null) return false;
            charger.Status = ChargerStatus.Online;
            foreach (var f in Faults.Where(f => f.ChargerId == id && f.Status != FaultStatus.Resolved))
            {
                f.Status = FaultStatus.Resolved;
                f.ResolvedAt = Now;
            }
            return true;
        }

        // ============================================================
        // ORDERS
        // ============================================================
        public PagedResult<ChargingOrderRow> SearchChargingOrders(string? keyword, string? statusFilter, int page, int pageSize = 10)
        {
            var query = ChargingOrders.Select(o =>
            {
                var cust = Customers.FirstOrDefault(c => c.Id == o.CustomerId);
                var charger = Chargers.FirstOrDefault(c => c.Id == o.ChargerId);
                var conn = charger?.Connectors.FirstOrDefault(c => c.Id == o.ConnectorId);
                var station = Stations.FirstOrDefault(s => s.Id == o.StationId);
                return new ChargingOrderRow
                {
                    Id = o.Id,
                    Code = o.Code,
                    CustomerName = cust?.FullName ?? "",
                    CustomerCode = cust?.Code ?? "",
                    ChargerCode = charger?.Code ?? "",
                    ConnectorCode = conn?.Code ?? "",
                    StationName = station?.Name ?? "",
                    StartTime = o.StartTime,
                    EndTime = o.EndTime,
                    EnergyKwh = o.EnergyKwh,
                    Amount = o.Amount,
                    PaymentMethod = o.PaymentMethod,
                    Status = o.Status
                };
            }).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim().ToLower();
                query = query.Where(r => r.Code.ToLower().Contains(k) || r.CustomerName.ToLower().Contains(k) || r.CustomerCode.ToLower().Contains(k));
            }
            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "all" && Enum.TryParse<ChargingOrderStatus>(statusFilter, true, out var st))
            {
                query = query.Where(r => r.Status == st);
            }

            var ordered = query.OrderByDescending(r => r.StartTime).ToList();
            return PagedResult<ChargingOrderRow>.Create(ordered, page, pageSize);
        }

        public PagedResult<TopUpOrderRow> SearchTopUpOrders(string? keyword, string? statusFilter, int page, int pageSize = 10)
        {
            var query = TopUpOrders.Select(o =>
            {
                var cust = Customers.FirstOrDefault(c => c.Id == o.CustomerId);
                return new TopUpOrderRow
                {
                    Id = o.Id,
                    Code = o.Code,
                    CustomerName = cust?.FullName ?? "",
                    CustomerCode = cust?.Code ?? "",
                    Amount = o.Amount,
                    Method = o.Method,
                    CreatedAt = o.CreatedAt,
                    Status = o.Status
                };
            }).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim().ToLower();
                query = query.Where(r => r.Code.ToLower().Contains(k) || r.CustomerName.ToLower().Contains(k) || r.CustomerCode.ToLower().Contains(k));
            }
            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "all" && Enum.TryParse<TopUpStatus>(statusFilter, true, out var st))
            {
                query = query.Where(r => r.Status == st);
            }

            var ordered = query.OrderByDescending(r => r.CreatedAt).ToList();
            return PagedResult<TopUpOrderRow>.Create(ordered, page, pageSize);
        }

        // ============================================================
        // CUSTOMERS
        // ============================================================
        public CustomerListViewModel SearchCustomers(string? keyword, string? statusFilter, int page, int pageSize = 10)
        {
            var query = Customers.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim().ToLower();
                query = query.Where(c => c.FullName.ToLower().Contains(k) || c.Code.ToLower().Contains(k) || c.Phone.Contains(k));
            }
            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "all" && Enum.TryParse<CustomerStatus>(statusFilter, true, out var st))
            {
                query = query.Where(c => c.Status == st);
            }
            var ordered = query.OrderByDescending(c => c.RegisteredAt).ToList();
            return new CustomerListViewModel
            {
                Keyword = keyword,
                StatusFilter = statusFilter,
                Result = PagedResult<Customer>.Create(ordered, page, pageSize)
            };
        }

        public Customer? GetCustomerById(int id) => Customers.FirstOrDefault(c => c.Id == id);

        public void AddCustomer(CustomerFormModel form)
        {
            var id = _nextCustomerId++;
            Customers.Add(new Customer
            {
                Id = id,
                Code = $"KH-{id:0000}",
                FullName = form.FullName,
                Phone = form.Phone,
                Email = form.Email,
                WalletBalance = form.WalletBalance,
                Status = form.Status,
                RegisteredAt = Now
            });
        }

        public bool UpdateCustomer(CustomerFormModel form)
        {
            var customer = GetCustomerById(form.Id);
            if (customer == null) return false;
            customer.FullName = form.FullName;
            customer.Phone = form.Phone;
            customer.Email = form.Email;
            customer.WalletBalance = form.WalletBalance;
            customer.Status = form.Status;
            return true;
        }

        public bool DeleteCustomer(int id)
        {
            var customer = GetCustomerById(id);
            if (customer == null) return false;
            Customers.Remove(customer);
            return true;
        }

        public bool AdjustBalance(int customerId, decimal amount, string note)
        {
            var customer = GetCustomerById(customerId);
            if (customer == null) return false;
            customer.WalletBalance = Math.Max(0, customer.WalletBalance + amount);
            return true;
        }

        // ============================================================
        // STATIONS
        // ============================================================
        public StationListViewModel SearchStations(string? keyword, string? statusFilter, int page, int pageSize = 10)
        {
            var query = Stations.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim().ToLower();
                query = query.Where(s => s.Code.ToLower().Contains(k) || s.Name.ToLower().Contains(k) || s.Address.ToLower().Contains(k));
            }
            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "all")
            {
                var activeOnly = statusFilter == "Active";
                query = query.Where(s => s.Active == activeOnly);
            }

            var ordered = query.OrderBy(s => s.Name).ToList();
            return new StationListViewModel
            {
                Keyword = keyword,
                StatusFilter = statusFilter,
                Result = PagedResult<Station>.Create(ordered, page, pageSize)
            };
        }

        public void AddStation(StationFormModel form)
        {
            var id = Stations.Count == 0 ? 1 : Stations.Max(s => s.Id) + 1;
            Stations.Add(new Station
            {
                Id = id,
                Code = $"TR-{id:00}",
                Name = form.Name,
                Address = form.Address,
                TimeZone = form.TimeZone,
                DefaultElectricityPrice = form.DefaultElectricityPrice,
                Active = form.Active
            });
        }

        public bool UpdateStation(StationFormModel form)
        {
            var station = Stations.FirstOrDefault(s => s.Id == form.Id);
            if (station == null) return false;
            station.Name = form.Name;
            station.Address = form.Address;
            station.TimeZone = form.TimeZone;
            station.DefaultElectricityPrice = form.DefaultElectricityPrice;
            station.Active = form.Active;
            return true;
        }

        public bool DeleteStation(int id)
        {
            var station = Stations.FirstOrDefault(s => s.Id == id);
            if (station == null) return false;
            Stations.Remove(station);
            return true;
        }

        // ============================================================
        // TOP-UP PACKAGES
        // ============================================================
        public TopUpPackageListViewModel SearchTopUpPackages(string? keyword, string? statusFilter, int page, int pageSize = 10)
        {
            var query = TopUpPackages.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(k));
            }
            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "all")
            {
                var activeOnly = statusFilter == "Active";
                query = query.Where(p => p.Active == activeOnly);
            }

            var ordered = query.OrderByDescending(p => p.Amount).ToList();
            return new TopUpPackageListViewModel
            {
                Keyword = keyword,
                StatusFilter = statusFilter,
                Result = PagedResult<TopUpPackage>.Create(ordered, page, pageSize)
            };
        }

        public void AddTopUpPackage(TopUpPackageFormModel form)
        {
            var id = TopUpPackages.Count == 0 ? 1 : TopUpPackages.Max(p => p.Id) + 1;
            TopUpPackages.Add(new TopUpPackage
            {
                Id = id,
                Name = form.Name,
                Amount = form.Amount,
                Bonus = form.Bonus,
                Active = form.Active,
                CreatedAt = Now
            });
        }

        public bool UpdateTopUpPackage(TopUpPackageFormModel form)
        {
            var pkg = TopUpPackages.FirstOrDefault(p => p.Id == form.Id);
            if (pkg == null) return false;
            pkg.Name = form.Name;
            pkg.Amount = form.Amount;
            pkg.Bonus = form.Bonus;
            pkg.Active = form.Active;
            return true;
        }

        public bool DeleteTopUpPackage(int id)
        {
            var pkg = TopUpPackages.FirstOrDefault(p => p.Id == id);
            if (pkg == null) return false;
            TopUpPackages.Remove(pkg);
            return true;
        }

        // ============================================================
        // RFID CARDS
        // ============================================================
        public RfidCardListViewModel SearchRfidCards(string? keyword, string? statusFilter, int page, int pageSize = 10)
        {
            var query = Cards.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim().ToLower();
                query = query.Where(c => c.CardId.ToLower().Contains(k) || c.UserName.ToLower().Contains(k));
            }
            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "all")
            {
                var activeOnly = statusFilter == "Active";
                query = query.Where(c => c.Status == (activeOnly ? CardStatus.Active : CardStatus.Inactive));
            }

            var ordered = query.OrderByDescending(c => c.CreatedDate).ToList();
            return new RfidCardListViewModel
            {
                Keyword = keyword,
                StatusFilter = statusFilter,
                Customers = Customers,
                Result = PagedResult<RfidCard>.Create(ordered, page, pageSize)
            };
        }

        public void AddRfidCard(RfidCardFormModel form)
        {
            var id = Cards.Count == 0 ? 1 : Cards.Max(c => c.Id) + 1;
            var user = Customers.FirstOrDefault(c => c.Id == form.UserId);
            Cards.Add(new RfidCard
            {
                Id = id,
                CardId = form.CardId,
                UserId = form.UserId,
                UserName = user?.FullName ?? "",
                Status = CardStatus.Active,
                CreatedDate = Now,
                ExpiryDate = form.ExpiryDate
            });
        }

        public bool DeleteRfidCard(int id)
        {
            var card = Cards.FirstOrDefault(c => c.Id == id);
            if (card == null) return false;
            Cards.Remove(card);
            return true;
        }

        public bool ToggleRfidCardStatus(int id)
        {
            var card = Cards.FirstOrDefault(c => c.Id == id);
            if (card == null) return false;
            card.Status = card.Status == CardStatus.Active ? CardStatus.Inactive : CardStatus.Active;
            return true;
        }

        // ============================================================
        // RESERVATIONS
        // ============================================================
        public ReservationListViewModel SearchReservations(string? keyword, string? statusFilter, int page, int pageSize = 10)
        {
            var query = Reservations.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim().ToLower();
                query = query.Where(r => r.ReservationId.ToLower().Contains(k) || r.UserName.ToLower().Contains(k) || r.ChargerCode.ToLower().Contains(k));
            }
            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "all")
            {
                if (Enum.TryParse<ReservationStatus>(statusFilter, true, out var status))
                {
                    query = query.Where(r => r.Status == status);
                }
            }

            var ordered = query.OrderByDescending(r => r.StartTime).ToList();
            return new ReservationListViewModel
            {
                Keyword = keyword,
                StatusFilter = statusFilter,
                Customers = Customers,
                Chargers = Chargers,
                Result = PagedResult<Reservation>.Create(ordered, page, pageSize)
            };
        }

        public (bool Success, string Message) CreateReservation(ReservationFormModel form)
        {
            var user = Customers.FirstOrDefault(c => c.Id == form.UserId);
            var charger = Chargers.FirstOrDefault(c => c.Id == form.ChargerId);
            if (user == null || charger == null)
            {
                return (false, "Vui lòng chọn khách hàng và trụ sạc hợp lệ.");
            }

            var station = Stations.FirstOrDefault(s => s.Id == charger.StationId);
            if (station == null || !station.Active)
            {
                return (false, "Trạm đang không hoạt động, không thể đặt chỗ.");
            }

            if (user.WalletBalance < 50000m)
            {
                return (false, "Khách hàng không đủ số dư để đặt trước.");
            }

            if (charger.Status != ChargerStatus.Online)
            {
                return (false, "Trụ sạc hiện không online.");
            }

            if (charger.Connectors.Any(c => c.Status == ConnectorStatus.Charging || c.Status == ConnectorStatus.Fault))
            {
                return (false, "Trụ sạc hiện không sẵn sàng để đặt trước.");
            }

            var start = form.StartTime;
            var end = start.AddMinutes(form.DurationMinutes);
            var id = Reservations.Count == 0 ? 1 : Reservations.Max(r => r.Id) + 1;
            Reservations.Add(new Reservation
            {
                Id = id,
                ReservationId = $"RES-{id:0000}",
                UserId = user.Id,
                UserName = user.FullName,
                ChargerId = charger.Id,
                ChargerCode = charger.Code,
                StartTime = start,
                EndTime = end,
                Status = ReservationStatus.Active
            });

            return (true, "Đặt chỗ thành công.");
        }

        // ============================================================
        // FINANCIAL MANAGEMENT
        // ============================================================
        public FinancialViewModel GetFinancialViewModel(string? keyword, int page, int pageSize = 10)
        {
            var totals = new FinancialOverview
            {
                TotalIncome = ChargingOrders.Where(o => o.Status != ChargingOrderStatus.Cancelled).Sum(o => o.Amount) + TopUpOrders.Where(o => o.Status == TopUpStatus.Success).Sum(o => o.Amount),
                TodayIncome = ChargingOrders.Where(o => o.StartTime >= Now.Date && o.StartTime < Now.Date.AddDays(1) && o.Status != ChargingOrderStatus.Cancelled).Sum(o => o.Amount),
                ThisMonthIncome = ChargingOrders.Where(o => o.StartTime >= new DateTime(Now.Year, Now.Month, 1) && o.StartTime < new DateTime(Now.Year, Now.Month, 1).AddMonths(1) && o.Status != ChargingOrderStatus.Cancelled).Sum(o => o.Amount) + TopUpOrders.Where(o => o.CreatedAt >= new DateTime(Now.Year, Now.Month, 1) && o.CreatedAt < new DateTime(Now.Year, Now.Month, 1).AddMonths(1) && o.Status == TopUpStatus.Success).Sum(o => o.Amount),
                AvailableBalance = Customers.Sum(c => c.WalletBalance) / 2m
            };

            var query = Transactions.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim().ToLower();
                query = query.Where(t => t.TransactionId.ToLower().Contains(k) || t.TransactionType.ToLower().Contains(k));
            }

            return new FinancialViewModel
            {
                Overview = totals,
                Keyword = keyword,
                Transactions = PagedResult<FinancialTransaction>.Create(query.OrderByDescending(t => t.TransactionTime).ToList(), page, pageSize)
            };
        }

        public bool WithdrawBalance(decimal amount, string bankAccount)
        {
            var totalBalance = Customers.Sum(c => c.WalletBalance);
            if (amount <= 0 || amount > totalBalance) return false;

            Transactions.Add(new FinancialTransaction
            {
                Id = Transactions.Count + 1,
                TransactionId = $"TX-{DateTime.Now.Year}-{Transactions.Count + 1:0000}",
                TransactionTime = Now,
                TransactionType = "Rút tiền",
                Amount = amount,
                BalanceAfter = totalBalance - amount,
                Note = $"Rút tiền về {bankAccount}"
            });

            foreach (var customer in Customers)
            {
                if (customer.WalletBalance > 0)
                {
                    customer.WalletBalance = Math.Max(0, customer.WalletBalance - amount / Math.Max(1, Customers.Count));
                }
            }

            return true;
        }

        // ============================================================
        // OPERATIONS (Vận hành & bảo trì)
        // ============================================================

        public OperationsViewModel GetOperations(string tab)
        {
            var vm = new OperationsViewModel { Tab = tab };

            vm.Faults = Faults.OrderByDescending(f => f.ReportedAt).Select(f =>
            {
                var charger = Chargers.FirstOrDefault(c => c.Id == f.ChargerId);
                var conn = charger?.Connectors.FirstOrDefault(c => c.Id == f.ConnectorId);
                return new FaultRow
                {
                    Id = f.Id,
                    Code = f.Code,
                    ChargerCode = charger?.Code ?? "",
                    ChargerName = charger?.Name ?? "",
                    ConnectorCode = conn?.Code,
                    Description = f.Description,
                    Severity = f.Severity,
                    Status = f.Status,
                    ReportedAt = f.ReportedAt,
                    ResolvedAt = f.ResolvedAt
                };
            }).ToList();

            vm.OfflineChargers = Chargers.Where(c => c.Status == ChargerStatus.Offline).Select(c =>
            {
                var station = Stations.FirstOrDefault(s => s.Id == c.StationId);
                var fault = Faults.Where(f => f.ChargerId == c.Id).OrderByDescending(f => f.ReportedAt).FirstOrDefault();
                return new OfflineChargerRow
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    StationName = station?.Name ?? "",
                    ConnectorCount = c.Connectors.Count,
                    SinceReportedFault = fault?.ReportedAt,
                    Reason = fault?.Description
                };
            }).ToList();

            vm.AbnormalOrders = AbnormalOrders.OrderByDescending(a => a.DetectedAt).Select(a =>
            {
                var cust = Customers.FirstOrDefault(c => c.Id == a.CustomerId);
                return new AbnormalOrderRow
                {
                    Id = a.Id,
                    Code = a.Code,
                    OrderType = a.OrderType,
                    OrderCode = a.OrderCode,
                    CustomerName = cust?.FullName ?? "",
                    CustomerCode = cust?.Code ?? "",
                    Amount = a.Amount,
                    Type = a.Type,
                    Note = a.Note,
                    Status = a.Status,
                    DetectedAt = a.DetectedAt,
                    ResolutionNote = a.ResolutionNote
                };
            }).ToList();

            return vm;
        }

        public bool ResolveFault(int id, string? note)
        {
            var fault = Faults.FirstOrDefault(f => f.Id == id);
            if (fault == null) return false;
            fault.Status = FaultStatus.Resolved;
            fault.ResolvedAt = Now;
            if (!string.IsNullOrWhiteSpace(note)) fault.Description += $" (Ghi chú xử lý: {note})";

            if (fault.ConnectorId.HasValue)
            {
                var charger = Chargers.FirstOrDefault(c => c.Id == fault.ChargerId);
                var conn = charger?.Connectors.FirstOrDefault(c => c.Id == fault.ConnectorId);
                if (conn != null && conn.Status == ConnectorStatus.Fault) conn.Status = ConnectorStatus.Available;
            }

            var chargerToCheck = Chargers.FirstOrDefault(c => c.Id == fault.ChargerId);
            if (chargerToCheck != null && chargerToCheck.Status == ChargerStatus.Offline)
            {
                bool stillHasOpenFault = Faults.Any(f => f.ChargerId == chargerToCheck.Id && f.Status != FaultStatus.Resolved);
                if (!stillHasOpenFault) chargerToCheck.Status = ChargerStatus.Online;
            }
            return true;
        }

        public bool ResolveAbnormalOrder(int id, string resolutionNote)
        {
            var a = AbnormalOrders.FirstOrDefault(x => x.Id == id);
            if (a == null) return false;
            a.Status = AbnormalOrderStatus.Resolved;
            a.ResolutionNote = resolutionNote;
            return true;
        }

        public bool AdjustAbnormalOrderAmount(int id, decimal newAmount, string note)
        {
            var a = AbnormalOrders.FirstOrDefault(x => x.Id == id);
            if (a == null) return false;

            if (a.OrderType == SourceOrderType.Charging)
            {
                var order = ChargingOrders.FirstOrDefault(o => o.Code == a.OrderCode);
                if (order != null) order.Amount = newAmount;
            }
            else
            {
                var order = TopUpOrders.FirstOrDefault(o => o.Code == a.OrderCode);
                if (order != null) order.Amount = newAmount;
            }

            a.Amount = newAmount;
            a.Status = AbnormalOrderStatus.Resolved;
            a.ResolutionNote = string.IsNullOrWhiteSpace(note) ? $"Đã điều chỉnh số tiền thành {FormatCurrency(newAmount)}." : note;
            return true;
        }

        public bool RefundAbnormalOrder(int id, decimal refundAmount, string note)
        {
            var a = AbnormalOrders.FirstOrDefault(x => x.Id == id);
            if (a == null) return false;

            var customer = GetCustomerById(a.CustomerId);
            if (customer != null) customer.WalletBalance += refundAmount;

            a.Status = AbnormalOrderStatus.Resolved;
            a.ResolutionNote = string.IsNullOrWhiteSpace(note)
                ? $"Đã hoàn {FormatCurrency(refundAmount)} vào ví khách hàng."
                : note;
            return true;
        }
    }
}
