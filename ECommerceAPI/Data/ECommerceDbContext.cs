using ECommerceAPI.Entities;
using ECommerceAPI.Enums;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Data
{
    public sealed class ECommerceDbContext : DbContext
    {
        private static readonly DateTime SeedCreatedAtUtc =
            new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options)
            : base(options)
        {
        }

        // Master Entities
        public DbSet<PaymentMethodMaster> PaymentMethods { get; set; }
        public DbSet<PaymentStatusMaster> PaymentStatuses { get; set; }
        public DbSet<OrderStatusMaster> OrderStatuses { get; set; }
        public DbSet<NotificationChannelMaster> NotificationChannels { get; set; }
        public DbSet<NotificationStatusMaster> NotificationStatuses { get; set; }

        // Transaction Entities
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerAddress> CustomerAddresses { get; set; }
        public DbSet<VerificationCode> VerificationCodes { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<NotificationQueueItem> NotificationQueueItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureMasterEntities(modelBuilder);
            ConfigureCustomerEntities(modelBuilder);
            ConfigureProductEntities(modelBuilder);
            ConfigureCartEntities(modelBuilder);
            ConfigureOrderEntities(modelBuilder);
            ConfigurePaymentEntities(modelBuilder);
            ConfigureNotificationEntities(modelBuilder);
            ConfigureAuditColumns(modelBuilder);

            SeedMasterData(modelBuilder);
            SeedCategories(modelBuilder);
            SeedProducts(modelBuilder);
        }


        private static void ConfigureMasterEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentMethodMaster>(entity =>
            {
                entity.ToTable("PaymentMethods");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasConversion<int>()
                    .ValueGeneratedNever();

                entity.Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(500);

                entity.HasIndex(x => x.Name)
                    .IsUnique();
            });


            modelBuilder.Entity<PaymentStatusMaster>(entity =>
            {
                entity.ToTable("PaymentStatuses");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasConversion<int>()
                    .ValueGeneratedNever();

                entity.Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(500);

                entity.HasIndex(x => x.Name)
                    .IsUnique();
            });


            modelBuilder.Entity<OrderStatusMaster>(entity =>
            {
                entity.ToTable("OrderStatuses");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasConversion<int>()
                    .ValueGeneratedNever();

                entity.Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(500);

                entity.HasIndex(x => x.Name)
                    .IsUnique();
            });


            modelBuilder.Entity<NotificationChannelMaster>(entity =>
            {
                entity.ToTable("NotificationChannels");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasConversion<int>()
                    .ValueGeneratedNever();

                entity.Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasIndex(x => x.Name)
                    .IsUnique();
            });


            modelBuilder.Entity<NotificationStatusMaster>(entity =>
            {
                entity.ToTable("NotificationStatuses");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasConversion<int>()
                    .ValueGeneratedNever();

                entity.Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(500);

                entity.HasIndex(x => x.Name)
                    .IsUnique();
            });
        }


        private static void ConfigureCustomerEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customers");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.FirstName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.LastName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Email)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.Property(x => x.PhoneNumber)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.PasswordHash)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.HasIndex(x => x.Email)
                    .IsUnique();

                entity.HasIndex(x => x.PhoneNumber)
                    .IsUnique();

                entity.HasOne(x => x.Cart)
                    .WithOne(x => x.Customer)
                    .HasForeignKey<Cart>(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(x => x.Addresses)
                    .WithOne(x => x.Customer)
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(x => x.RefreshTokens)
                    .WithOne(x => x.Customer)
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(x => x.Orders)
                    .WithOne(x => x.Customer)
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<CustomerAddress>(entity =>
            {
                entity.ToTable("CustomerAddresses");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.FullName)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.PhoneNumber)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.AddressLine1)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.AddressLine2)
                    .HasMaxLength(200);

                entity.Property(x => x.City)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.State)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.PostalCode)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.Country)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasIndex(x => x.CustomerId);
            });


            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.TokenHash)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRowVersion();

                entity.HasIndex(x => x.TokenHash)
                    .IsUnique();

                entity.HasIndex(x => x.CustomerId);
            });

            modelBuilder.Entity<VerificationCode>(entity =>
            {
                entity.ToTable("VerificationCodes");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Purpose)
                      .HasConversion<string>()
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(x => x.Identifier)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.Property(x => x.CodeHash)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRowVersion();

                entity.HasIndex(x => new
                {
                    x.Purpose,
                    x.Identifier
                });

                entity.HasOne(x => x.Customer)
                    .WithMany(x => x.VerificationCodes)
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }


        private static void ConfigureProductEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.HasIndex(x => x.Name)
                    .IsUnique();

                entity.HasMany(x => x.Products)
                    .WithOne(x => x.Category)
                    .HasForeignKey(x => x.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(2000)
                    .IsRequired();

                entity.Property(x => x.Sku)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Price)
                    .HasPrecision(18, 2);

                entity.Property(x => x.ImageUrl)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRowVersion();

                entity.HasIndex(x => x.Sku)
                    .IsUnique();

                entity.HasIndex(x => new
                {
                    x.CategoryId,
                    x.IsActive
                });
            });
        }


        private static void ConfigureCartEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.ToTable("Carts");

                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.CustomerId)
                    .IsUnique();

                entity.Property(x => x.RowVersion)
                    .IsRowVersion();

                entity.HasMany(x => x.Items)
                    .WithOne(x => x.Cart)
                    .HasForeignKey(x => x.CartId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.ToTable("CartItems");

                entity.HasKey(x => x.Id);

                entity.HasIndex(x => new
                {
                    x.CartId,
                    x.ProductId
                })
                .IsUnique();

                entity.HasOne(x => x.Product)
                    .WithMany(x => x.CartItems)
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }


        private static void ConfigureOrderEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.OrderNumber)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Subtotal)
                    .HasPrecision(18, 2);

                entity.Property(x => x.TaxAmount)
                    .HasPrecision(18, 2);

                entity.Property(x => x.ShippingAmount)
                    .HasPrecision(18, 2);

                entity.Property(x => x.GrandTotal)
                    .HasPrecision(18, 2);

                entity.Property(x => x.Currency)
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(x => x.ShippingAddress)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.Property(x => x.BillingAddress)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.HasIndex(x => x.OrderNumber)
                    .IsUnique();

                entity.Property(x => x.RowVersion)
                    .IsRowVersion();

                entity.HasIndex(x => new
                {
                    x.CustomerId,
                    x.CheckoutToken
                }).IsUnique();

                entity.HasIndex(x => new
                {
                    x.CustomerId,
                    x.CreatedAtUtc
                });

                entity.HasOne(x => x.PaymentMethod)
                    .WithMany()
                    .HasForeignKey(x => x.PaymentMethodId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.PaymentStatus)
                    .WithMany()
                    .HasForeignKey(x => x.PaymentStatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.OrderStatus)
                    .WithMany()
                    .HasForeignKey(x => x.OrderStatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(x => x.Items)
                    .WithOne(x => x.Order)
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(x => x.Payments)
                    .WithOne(x => x.Order)
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(x => x.StatusHistories)
                    .WithOne(x => x.Order)
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.ProductName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.ProductSku)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.ProductImageUrl)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.Property(x => x.UnitPrice)
                    .HasPrecision(18, 2);

                entity.Property(x => x.LineTotal)
                    .HasPrecision(18, 2);

                entity.HasOne<Product>()
                    .WithMany()
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.OrderId);
            });


            modelBuilder.Entity<OrderStatusHistory>(entity =>
            {
                entity.ToTable("OrderStatusHistories");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Remarks)
                    .HasMaxLength(500);

                entity.HasOne(x => x.PreviousStatus)
                    .WithMany()
                    .HasForeignKey(x => x.PreviousStatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.NewStatus)
                    .WithMany()
                    .HasForeignKey(x => x.NewStatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new
                {
                    x.OrderId,
                    x.CreatedAtUtc
                });
            });
        }


        private static void ConfigurePaymentEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.ToTable("PaymentTransactions");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Provider)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.ProviderTransactionId)
                    .HasMaxLength(200);

                entity.Property(x => x.Amount)
                    .HasPrecision(18, 2);

                entity.Property(x => x.Currency)
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(x => x.MaskedInstrument)
                    .HasMaxLength(100);

                entity.Property(x => x.FailureReason)
                    .HasMaxLength(500);

                entity.Property(x => x.RowVersion)
                    .IsRowVersion();

                entity.HasOne(x => x.PaymentMethod)
                    .WithMany()
                    .HasForeignKey(x => x.PaymentMethodId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.PaymentStatus)
                    .WithMany()
                    .HasForeignKey(x => x.PaymentStatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.OrderId);

                entity.HasIndex(x => x.ProviderTransactionId)
                    .IsUnique()
                    .HasFilter("[ProviderTransactionId] IS NOT NULL");
            });
        }


        private static void ConfigureNotificationEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotificationQueueItem>(entity =>
            {
                entity.ToTable("NotificationQueueItems");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Recipient)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.Property(x => x.Subject)
                    .HasMaxLength(250);

                entity.Property(x => x.Body)
                    .HasMaxLength(4000)
                    .IsRequired();

                entity.Property(x => x.LastError)
                    .HasMaxLength(2000);

                entity.HasIndex(x => new
                {
                    x.NotificationStatusId,
                    x.NextAttemptAtUtc
                });

                entity.HasIndex(x => new
                {
                    x.NotificationStatusId,
                    x.ProcessingStartedAtUtc
                });

                entity.HasOne(x => x.NotificationChannel)
                    .WithMany()
                    .HasForeignKey(x => x.NotificationChannelId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.NotificationStatus)
                    .WithMany()
                    .HasForeignKey(x => x.NotificationStatusId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }


        // Instead of repeating the same Fluent API configuration
        // for these four properties in every entity configuration,
        // ConfigureAuditColumns finds every entity derived from AuditableEntity
        // and applies the configuration automatically.
        private static void ConfigureAuditColumns(ModelBuilder modelBuilder)
        {
            // Find all entity types that inherit from AuditableEntity
            var auditableEntityTypes = modelBuilder.Model
                .GetEntityTypes()
                .Where(x => typeof(AuditableEntity).IsAssignableFrom(x.ClrType));

            // Apply the common audit-column configuration to each entity
            foreach (var entityType in auditableEntityTypes)
            {
                var entity = modelBuilder.Entity(entityType.ClrType);

                // Configure the CreatedAtUtc column
                entity.Property(nameof(AuditableEntity.CreatedAtUtc))
                    .HasColumnType("datetime2");

                // Configure the maximum length of CreatedBy
                entity.Property(nameof(AuditableEntity.CreatedBy))
                    .HasMaxLength(100);

                // Configure the UpdatedAtUtc column
                entity.Property(nameof(AuditableEntity.UpdatedAtUtc))
                    .HasColumnType("datetime2");

                // Configure the maximum length of UpdatedBy
                entity.Property(nameof(AuditableEntity.UpdatedBy))
                    .HasMaxLength(100);
            }
        }


        private static void SeedMasterData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentMethodMaster>().HasData(
                new
                {
                    Id = PaymentMethod.CashOnDelivery,
                    Name = "Cash on Delivery",
                    Description = "Pay when the order is delivered.",
                    RequiresOnlinePayment = false,
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },
                new
                {
                    Id = PaymentMethod.Card,
                    Name = "Card",
                    Description = "Pay using a Debit or Credit Card.",
                    RequiresOnlinePayment = true,
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },
                new
                {
                    Id = PaymentMethod.Upi,
                    Name = "UPI",
                    Description = "Pay using UPI.",
                    RequiresOnlinePayment = true,
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                });


            modelBuilder.Entity<PaymentStatusMaster>().HasData(
                new
                {
                    Id = PaymentStatus.Pending,
                    Name = "Pending",
                    Description = "Payment is pending.",
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },
                new
                {
                    Id = PaymentStatus.Paid,
                    Name = "Paid",
                    Description = "Payment completed successfully.",
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },
                new
                {
                    Id = PaymentStatus.Failed,
                    Name = "Failed",
                    Description = "Payment failed.",
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },
                new
                {
                    Id = PaymentStatus.Refunded,
                    Name = "Refunded",
                    Description = "Payment was refunded.",
                    IsActive = true,
                    DisplayOrder = 4,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                });


            modelBuilder.Entity<OrderStatusMaster>().HasData(
                new
                {
                    Id = OrderStatus.Pending,
                    Name = "Pending",
                    Description = "Order is waiting for confirmation.",
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },
                new
                {
                    Id = OrderStatus.Confirmed,
                    Name = "Confirmed",
                    Description = "Order has been confirmed.",
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },
                new
                {
                    Id = OrderStatus.Processing,
                    Name = "Processing",
                    Description = "Order is being processed.",
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },
                new
                {
                    Id = OrderStatus.Shipped,
                    Name = "Shipped",
                    Description = "Order has been shipped.",
                    IsActive = true,
                    DisplayOrder = 4,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },
                new
                {
                    Id = OrderStatus.Delivered,
                    Name = "Delivered",
                    Description = "Order has been delivered.",
                    IsActive = true,
                    DisplayOrder = 5,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },
                new
                {
                    Id = OrderStatus.Cancelled,
                    Name = "Cancelled",
                    Description = "Order has been cancelled.",
                    IsActive = true,
                    DisplayOrder = 6,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },
                new
                {
                    Id = OrderStatus.PaymentFailed,
                    Name = "Payment Failed",
                    Description = "Order payment failed.",
                    IsActive = true,
                    DisplayOrder = 7,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                });


            modelBuilder.Entity<NotificationChannelMaster>().HasData(
                new
                {
                    Id = NotificationChannel.Email,
                    Name = "Email",
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },
                new
                {
                    Id = NotificationChannel.Sms,
                    Name = "SMS",
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                });


            modelBuilder.Entity<NotificationStatusMaster>().HasData(
                new
                {
                    Id = NotificationStatus.Pending,
                    Name = "Pending",
                    Description =
                        "Notification is waiting to be processed.",
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },
                new
                {
                    Id = NotificationStatus.Sent,
                    Name = "Sent",
                    Description =
                        "Notification was sent successfully.",
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },
                new
                {
                    Id = NotificationStatus.Failed,
                    Name = "Failed",
                    Description =
                        "Notification could not be sent.",
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },
                new
                {
                    Id = NotificationStatus.Processing,
                    Name = "Processing",
                    Description = "Notification has been claimed by a processor and is being sent.",
                    IsActive = true,
                    DisplayOrder = 4,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                });
        }


        private static void SeedCategories(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new
                {
                    Id = 1,
                    Name = "Electronics",
                    Description = "Laptops, audio devices, smart devices and cameras.",
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },
                new
                {
                    Id = 2,
                    Name = "Mobiles",
                    Description = "Smartphones and mobile devices.",
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },
                new
                {
                    Id = 3,
                    Name = "Fashion",
                    Description = "Footwear and fashion products.",
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },
                new
                {
                    Id = 4,
                    Name = "Bags",
                    Description = "Backpacks and travel bags.",
                    IsActive = true,
                    DisplayOrder = 4,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                });
        }


        private static void SeedProducts(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasData(
                new
                {
                    Id = 1,
                    CategoryId = 1,
                    Name = "Performance Laptop",
                    Description = "Powerful laptop suitable for development, office work and everyday computing.",
                    Sku = "LAPTOP-001",
                    Price = 74999.00m,
                    StockQuantity = 25,
                    ImageUrl = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?auto=format&fit=crop&w=900&q=80",
                    IsActive = true,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },

                new
                {
                    Id = 2,
                    CategoryId = 1,
                    Name = "Wireless Headphones",
                    Description = "Comfortable wireless headphones with high-quality sound and long battery life.",
                    Sku = "HEADPHONE-001",
                    Price = 7999.00m,
                    StockQuantity = 50,
                    ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=900&q=80",
                    IsActive = true,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },

                new
                {
                    Id = 3,
                    CategoryId = 1,
                    Name = "Smart Watch",
                    Description = "Smart wearable device for notifications, fitness tracking and daily activity monitoring.",
                    Sku = "WATCH-001",
                    Price = 12999.00m,
                    StockQuantity = 35,
                    ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=900&q=80",
                    IsActive = true,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },

                new
                {
                    Id = 4,
                    CategoryId = 1,
                    Name = "Mirrorless Camera",
                    Description = "Compact digital camera suitable for professional photography and video recording.",
                    Sku = "CAMERA-001",
                    Price = 64999.00m,
                    StockQuantity = 15,
                    ImageUrl = "https://images.unsplash.com/photo-1516035069371-29a1b244cc32?auto=format&fit=crop&w=900&q=80",
                    IsActive = true,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },

                new
                {
                    Id = 5,
                    CategoryId = 2,
                    Name = "5G Smartphone",
                    Description = "Modern 5G smartphone with a bright display, fast performance and quality camera.",
                    Sku = "MOBILE-001",
                    Price = 34999.00m,
                    StockQuantity = 40,
                    ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?auto=format&fit=crop&w=900&q=80",
                    IsActive = true,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },

                new
                {
                    Id = 6,
                    CategoryId = 2,
                    Name = "Premium Smartphone",
                    Description = "Premium smartphone with a high-resolution display, powerful processor and advanced camera.",
                    Sku = "MOBILE-002",
                    Price = 59999.00m,
                    StockQuantity = 30,
                    ImageUrl = "https://images.unsplash.com/photo-1598327105666-5b89351aff97?auto=format&fit=crop&w=900&q=80",
                    IsActive = true,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },

                new
                {
                    Id = 7,
                    CategoryId = 3,
                    Name = "Running Shoes",
                    Description = "Lightweight running shoes designed for comfort, fitness and everyday use.",
                    Sku = "SHOES-001",
                    Price = 4999.00m,
                    StockQuantity = 60,
                    ImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=900&q=80",
                    IsActive = true,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },

                new
                {
                    Id = 8,
                    CategoryId = 3,
                    Name = "Casual T-Shirt",
                    Description = "Comfortable casual T-shirt suitable for everyday wear.",
                    Sku = "TSHIRT-001",
                    Price = 1499.00m,
                    StockQuantity = 80,
                    ImageUrl = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?auto=format&fit=crop&w=900&q=80",
                    IsActive = true,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },

                new
                {
                    Id = 9,
                    CategoryId = 4,
                    Name = "Travel Backpack",
                    Description = "Spacious backpack suitable for office, college and travel.",
                    Sku = "BAG-001",
                    Price = 2999.00m,
                    StockQuantity = 45,
                    ImageUrl = "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?auto=format&fit=crop&w=900&q=80",
                    IsActive = true,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                },

                new
                {
                    Id = 10,
                    CategoryId = 4,
                    Name = "Premium Handbag",
                    Description = "Stylish handbag suitable for daily use, office and travel.",
                    Sku = "BAG-002",
                    Price = 4499.00m,
                    StockQuantity = 35,
                    ImageUrl = "https://images.unsplash.com/photo-1584917865442-de89df76afd3?auto=format&fit=crop&w=900&q=80",
                    IsActive = true,
                    CreatedAtUtc = SeedCreatedAtUtc,
                    CreatedBy = "System"
                });
        }
    }
}
