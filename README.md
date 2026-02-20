# 🏬 Product Inventory Management System (PIMS)

A production-ready Product Inventory Management System built using:

- .NET 9 Web API
- Entity Framework Core (Code First)
- SQL Server
- JWT Authentication
- Role-Based Authorization (Administrator, User)
- API Versioning
- Serilog Logging
- In-Memory Caching
- Clean Architecture
- Async Programming

---

## 📌 Project Overview

PIMS is a secure, versioned RESTful Web API that manages:

- Products
- Categories
- Inventory
- Inventory Transactions
- Price Adjustments
- Low Stock Monitoring
- User Authentication & RBAC

---

## 🏗 Architecture

The project follows **Clean Architecture** principles:


PIMS
│
├── PIMS.API → Presentation Layer
├── PIMS.Application → Business Logic Layer
├── PIMS.Domain → Core Entities
├── PIMS.Infrastructure → Data Access & External Services


### Principles Applied

- SOLID
- Separation of Concerns
- Dependency Injection
- Service Layer Pattern
- DTO Projection
- Global Exception Handling

---

## 🔐 Authentication & Authorization

- JWT-based authentication
- ASP.NET Identity
- Role-Based Access Control
- Admin-only endpoints protected
- Secure password hashing

Roles:
- Administrator
- User

---

## 📦 Core Features

### Product Management

- Create products (Admin only)
- Unique SKU enforcement
- Multiple categories (Many-to-Many)
- Search, filter, pagination, sorting

### Price Adjustment Engine

- Individual price update
- Bulk price adjustment
- Percentage or Fixed reduction
- Negative price protection
- Audit trail for price changes
- Transaction safety

### Inventory Management

- Inventory adjustment transactions
- Prevent negative stock
- Low stock detection
- Inventory transaction history
- Audit trail tracking

### Caching

- In-memory caching for product list
- Automatic cache invalidation on updates

### Logging

- Structured logging using Serilog
- Authentication logs
- Inventory logs
- Price update logs

---

## 🗄 Database Design

Tables:

- Products
- Categories
- ProductCategories
- Inventories
- InventoryTransactions
- ProductPriceHistories
- AspNetUsers / Roles (Identity)

Key Constraints:

- Unique SKU index
- Cascade delete for price history
- One-to-One Product → Inventory
- Many-to-Many Product ↔ Category

---
