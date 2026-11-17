# E-Commerce RESTful Web API

A full-lifecycle E-Commerce backend built with **ASP.NET Core Web API (.NET 10)**, **C#**, **Entity Framework Core**, and **SQL Server**. This project implements production-style security, transactional order processing, simulated payment workflows, and asynchronous notifications.

## 🛠 Tech Stack
* **Framework:** ASP.NET Core Web API (.NET 10)
* **Language:** C#
* **ORM & Database:** Entity Framework Core, SQL Server
* **Authentication & Security:** JWT Bearer Authentication, Hashed Refresh Tokens, Two-Factor Authentication (2FA)
* **External Integrations:** Google SMTP (Email), Twilio (SMS)
* **API Documentation:** Swagger / OpenAPI

## ✨ Key Features

### 🔐 Authentication & Security
* **JWT & Refresh Token Flow:** Short-lived JWT Access Tokens paired with long-lived Refresh Tokens stored securely as hashed values in SQL Server, supporting single-session logout revocation.
* **Account Verification:** Time-bound Email (Google SMTP) and Mobile (Twilio) verification codes persisted in SQL Server with expiration windows and failed-attempt limits.
* **Two-Factor Authentication (2FA):** Optional Email-based 2FA during login, strictly gated until both Email and Mobile numbers are verified.

### 🛍 Product Catalog & Shopping Cart
* **Public Browsing:** Search products by keyword, filter by category, price range, and stock availability, with sorting and pagination.
* **Authenticated Cart:** Add/remove items, adjust quantities, clear cart, and dynamically calculate line totals based on current product pricing.
* **Address Management:** Support for multiple customer addresses with default shipping and billing designations.

### 💳 Checkout, Orders & Payment Simulator
* **Transactional Order Placement:** Validates customer status, cart items, and real-time stock quantities before deducting inventory and clearing the cart.
* **Payment Simulator:** Supports **Card**, **UPI**, and **Cash on Delivery (COD)** workflows with **Paid**, **Failed**, and **Pending** states. Sensitive payment details are masked (e.g., `**** **** **** 1234`) before storage.
* **Order Lifecycle Tracking:** Complete order history and audit trail (`OrderStatusHistory`) tracking every status transition with timestamps and remarks.

### 📩 Asynchronous Notification Queue
* Decouples Email and SMS alerts from core checkout transactions using a database-backed **Notification Queue** with status tracking (`Pending`, `Sent`, `Failed`) and automated retry handling.

## 🚀 Getting Started

### Prerequisites
* [.NET 10 SDK](https://microsoft.com)
* SQL Server (LocalDB, Express, or Docker)

### Setup Instructions
1. **Clone the repository:**
   ```bash
   git clone https://github.com
   cd ECommerceAPI_Application/ECommerceAPI
   ```
2. **Configure `appsettings.json`:**
   Update your database connection string and external service credentials:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=ECommerceDB;Trusted_Connection=True;TrustServerCertificate=True;"
     },
     "Jwt": {
       "Key": "YOUR_SECRET_KEY",
       "Issuer": "ECommerceAPI",
       "Audience": "ECommerceClient"
     },
     "SmtpSettings": {
       "Host": "://gmail.com",
       "Port": 587,
       "Email": "your-email@gmail.com",
       "Password": "your-app-password"
     },
     "Twilio": {
       "AccountSid": "YOUR_TWILIO_SID",
       "AuthToken": "YOUR_TWILIO_AUTH_TOKEN",
       "FromPhoneNumber": "+1234567890"
     }
   }
   ```
3. **Apply Database Migrations:**
   ```bash
   dotnet ef database update
   ```
4. **Run the Application:**
   ```bash
   dotnet run
   ```
   Navigate to `https://localhost:<port>/swagger` to explore and test the API endpoints.
