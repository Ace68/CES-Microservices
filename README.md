# SQL Server 2025 Change Event Streaming - Microservices Demo

This repository demonstrates practical implementations of **Change Event Streaming (CES)**, a groundbreaking feature in SQL Server 2025 that enables real-time data synchronization between microservices through Azure Event Hubs.

## Overview

Change Event Streaming allows SQL Server to continuously stream row-level changes from tables directly into Azure Event Hubs as CloudEvents. This enables event-driven architectures where multiple consumer applications can subscribe to data changes in real time, facilitating seamless microservice communication without tight coupling.

## Architecture

This solution consists of two independent microservices, each with its own database:

### 1. Warehouse Microservice (`src/BrewUpWarehouse`)

- Manages product inventory and warehouse operations
- Database: SQL Server (Warehouse DB)
- Publishes product changes via Change Event Streaming
- Subscribes to sales order integration events

### 2. Sales Microservice (`src/BrewUpSales`)

- Handles sales orders using CQRS-ES pattern
- Database: SQL Server (Sales DB)
- Subscribes to product changes from Warehouse
- Publishes sales order events via Change Event Streaming

## Demo Scenarios

### Scenario 1: Product Synchronization (Warehouse → Sales)

**Flow:**

1. A new product is created through the Warehouse API
2. Product data is persisted in the Warehouse SQL Server database
3. Change Event Streaming automatically publishes a CloudEvent to Azure Event Hub (`producthub`)
4. Sales microservice subscribes to the Event Hub
5. Sales microservice receives the notification and updates its Product table in the Sales database

**Key Technologies:**

- SQL Server Change Event Streaming on `dbo.Product` table
- Azure Event Hub as message broker
- CloudEvents standard for event format

**Use Case:** Ensures product catalog consistency across microservices without direct API calls or database coupling.

### Scenario 2: Event Sourcing with Integration Events (Sales → Warehouse)

**Flow:**

1. A new sales order is created through the Sales API
2. Using CQRS-ES pattern, a `SalesOrderCreated` domain event is raised
3. Event is persisted in the EventStore table (SQL Server - Sales DB)
4. Change Event Streaming publishes the event to Azure Event Hub (`eventstorehub`)
5. Sales microservice subscribes to its own Event Hub to:
   - Update the read model (SalesOrder table in Sales DB)
   - Generate an integration event for Warehouse
6. Warehouse microservice subscribes to the integration event and processes it

**Key Technologies:**

- CQRS-ES (Command Query Responsibility Segregation - Event Sourcing)
- SQL Server Change Event Streaming on `dbo.EventStore` table
- Azure Event Hub for event distribution
- Event-driven integration between bounded contexts

**Use Case:** Implements event sourcing with automatic event distribution, enabling audit trails, temporal queries, and reliable cross-service communication.

## Architecture Diagram

```text
┌─────────────────────────────────────────────────────────────────────┐
│                         Azure Event Hubs                            │
│  ┌──────────────────────┐          ┌──────────────────────┐        │
│  │   producthub         │          │   eventstorehub      │        │
│  └──────────────────────┘          └──────────────────────┘        │
└─────────────────────────────────────────────────────────────────────┘
           ▲                                    ▲         │
           │ CloudEvents                        │         │ CloudEvents
           │                                    │         │
┌──────────┴────────────┐           ┌───────────┴─────────▼───────────┐
│  Warehouse Service    │           │     Sales Service (CQRS-ES)     │
│  ─────────────────    │           │     ─────────────────────       │
│  • Product API        │           │     • Sales Order API           │
│  • SQL Server DB      │           │     • EventStore (Write)        │
│    - dbo.Product      │           │     • SalesOrder (Read Model)   │
│      [CES Enabled]    │           │     • SQL Server DB             │
│                       │           │       - dbo.EventStore          │
│  Publishes Product    │           │         [CES Enabled]           │
│  changes              │           │       - dbo.SalesOrder          │
└───────────────────────┘           │       - dbo.Product             │
                                    │                                 │
                                    │  Subscribes to producthub      │
                                    └─────────────────────────────────┘

Scenario 1: Warehouse creates Product → CES → Event Hub → Sales subscribes
Scenario 2: Sales creates Order → EventStore → CES → Event Hub → Sales/Warehouse subscribe
```

## Prerequisites

Before starting with Change Event Streaming setup, ensure you have:

### Azure Requirements

- **Azure Subscription**: Active subscription with appropriate permissions
- **Azure Event Hubs**: Namespace and Event Hub creation rights
- **PowerShell Modules**: Az and Az.EventHub modules (installation covered below)

### SQL Server Requirements

- **SQL Server 2025** (or later) with Change Event Streaming support
- [SQL Server limitations](https://learn.microsoft.com/en-us/sql/relational-databases/track-changes/change-event-streaming/configure?view=sql-server-ver17#limitations)

### Development Tools

- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code
- Azure Storage Explorer (optional, for monitoring Event Hub checkpoints)

## Setup Instructions

### 1. Create Event Hubs

Navigate to Azure Portal and create two Event Hubs:

1. **producthub** - for Warehouse product changes
2. **eventstorehub** - for Sales event store changes

![Create EventHub](images/CreateEventHub.jpg)

Create a new Entity:

![Create Event Hub Entity](images/EventHub-CreateEventHub.jpg)

Create a new Policy with **Send** and **Listen** permissions:

![Create Custom Policy](images/EventHub-CreatePolicy.jpg)

### 2. Generate SAS Token

You'll need a Shared Access Signature (SAS) token for SQL Server authentication against Event Hubs.

#### Install PowerShell Modules

Run PowerShell as an administrator:

```powershell
Install-Module -Name Az -Scope CurrentUser -Repository PSGallery -Force
Install-Module -Name Az.EventHub -Scope CurrentUser -Force
```

#### Run the SAS Token Script

Two scripts are provided in the `sqlScripts` folder:

- `Generate-SAS-Token.ps1` - for eventstorehub (Sales)
- `Generate-Warehouse-SAS-Token.ps1` - for producthub (Warehouse)

Before executing:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
```

Then run:

```powershell
.\sqlScripts\Generate-SAS-Token.ps1
.\sqlScripts\Generate-Warehouse-SAS-Token.ps1
```

The script will generate a SAS token and copy it to your clipboard.

### 3. Configure SQL Server - Sales Database

Execute the following scripts in order for the **Sales** database:

#### Enable Change Event Streaming

```sql
-- sqlScripts/EnableChangeEventStream.sql
CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'YourStrongPassword!';

CREATE DATABASE SCOPED CREDENTIAL SqlCesCredential
WITH 
  IDENTITY = 'SHARED ACCESS SIGNATURE',
  SECRET = '<Your SAS Token for eventstorehub>';

EXEC sys.sp_enable_event_stream;

-- Verify
SELECT * FROM sys.databases WHERE is_event_stream_enabled = 1;
```

#### Create Event Stream Group for EventStore

```sql
-- sqlScripts/AddStreamGroupToEventStore.sql
EXEC sys.sp_create_event_stream_group
  @stream_group_name      = 'ces-wpc-2025',
  @destination_location   = 'wpc-eventhub.servicebus.windows.net/eventstorehub',
  @destination_credential = SqlCesCredential,
  @destination_type       = 'AzureEventHubsAmqp';

EXEC sys.sp_add_object_to_event_stream_group
  @stream_group_name = 'ces-wpc-2025',
  @object_name = 'dbo.EventStore',
  @include_old_values = 0,
  @include_all_columns = 1;

-- Verify
EXEC sp_help_change_feed_table @source_schema = 'dbo', @source_name = 'EventStore';
```

### 4. Configure SQL Server - Warehouse Database

Execute the following scripts for the **Warehouse** database:

#### Enable Change Event Streaming

```sql
CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'YourStrongPassword!';

CREATE DATABASE SCOPED CREDENTIAL SqlWarehouseCredential
WITH 
  IDENTITY = 'SHARED ACCESS SIGNATURE',
  SECRET = '<Your SAS Token for producthub>';

EXEC sys.sp_enable_event_stream;
```

#### Create Event Stream Group for Product

```sql
-- sqlScripts/AddStreamGroupToProduct.sql
EXEC sys.sp_create_event_stream_group
  @stream_group_name      = 'warehouse-ces-2025',
  @destination_location   = 'wpc-eventhub.servicebus.windows.net/producthub',
  @destination_credential = SqlWarehouseCredential,
  @destination_type       = 'AzureEventHubsAmqp';

EXEC sys.sp_add_object_to_event_stream_group
  @stream_group_name      = 'warehouse-ces-2025',
  @object_name = 'dbo.Product',
  @include_old_values = 1,
  @include_all_columns = 1;

-- Verify
EXEC sp_help_change_feed_table @source_schema = 'dbo', @source_name = 'Product';
```

### 5. Configure Application Settings

Update the `appsettings.json` files in both microservices with your connection strings and Event Hub details.

**Sales API** (`src/BrewUpSales/BrewUp.Rest/appsettings.json`):

```json
{
  "ConnectionStrings": {
    "SalesConnection": "Your SQL Server Connection String"
  },
  "EventHub": {
    "ConnectionString": "Endpoint=sb://wpc-eventhub.servicebus.windows.net/...",
    "EventHubName": "eventstorehub",
    "ConsumerGroup": "$Default",
    "BlobStorageConnectionString": "Your Blob Storage Connection String",
    "BlobContainerName": "checkpoints"
  }
}
```

**Warehouse API** (`src/BrewUpWarehouse/BrewUp.Rest/appsettings.json`):

```json
{
  "ConnectionStrings": {
    "WarehouseConnection": "Your SQL Server Connection String"
  },
  "EventHub": {
    "ConnectionString": "Endpoint=sb://wpc-eventhub.servicebus.windows.net/...",
    "EventHubName": "producthub",
    "ConsumerGroup": "$Default",
    "BlobStorageConnectionString": "Your Blob Storage Connection String",
    "BlobContainerName": "checkpoints"
  }
}
```

### 6. Run the Applications

Open the solutions in Visual Studio or use the command line:

```bash
# Terminal 1 - Sales API
cd src/BrewUpSales
dotnet run --project BrewUp.Rest

# Terminal 2 - Warehouse API
cd src/BrewUpWarehouse
dotnet run --project BrewUp.Rest
```

## Testing the Scenarios

### Test Scenario 1: Product Synchronization

1. Create a product via Warehouse API:

```bash
POST http://localhost:5001/api/products
Content-Type: application/json

{
  "productId": "prod-001",
  "name": "Premium IPA",
  "description": "Hoppy craft beer",
  "unitPrice": 5.99,
  "itemsInStock": 100
}
```

2. Verify the product appears in the Sales database `Product` table
3. Check Event Hub metrics in Azure Portal for messages received

### Test Scenario 2: Sales Order with Event Sourcing

1. Create a sales order via Sales API:

Use the provided JSON file:

```bash
POST http://localhost:5000/api/sales/orders
Content-Type: application/json

# Use sqlScripts/CreateSalesOrder.json
```

2. Verify:
   - Event is saved in `dbo.EventStore` (Sales DB)
   - Read model updated in `dbo.SalesOrder` (Sales DB)
   - Event appears in Event Hub
   - Warehouse processes the integration event

## CloudEvent Format

Change Event Streaming publishes events in CloudEvents format. Example:

```json
{
  "specversion": "1.0",
  "type": "com.microsoft.SQL.CES.DML.V1",
  "source": "/",
  "id": "cc3fcdca-09c0-4f46-a8d3-5d0c3c1eb85a",
  "time": "2025-11-03T12:29:46.290Z",
  "datacontenttype": "application/avro-json",
  "operation": "INS",
  "segmentindex": 1,
  "finalsegment": true,
  "data": {
    "eventsource": {
      "db": "WpcDemo",
      "schema": "dbo",
      "tbl": "Product",
      "cols": [
        {
          "name": "ProductId",
          "type": "int",
          "index": 0
        }
      ],
      "pkkey": [
        {
          "columnname": "ProductId",
          "value": "2"
        }
      ],
      "transaction": {
        "commitlsn": "0000002C:00000730:0011",
        "beginlsn": "0000002C:00000730:000C",
        "sequencenumber": 2,
        "committime": "2025-06-30T12:29:46.290Z"
      }
    },
    "eventrow": {
      "old": null,
      "current": "{\"ProductId\": \"2\", \"Name\": \"Premium IPA\"}"
    }
  }
}
```

## Troubleshooting

### Common Issues and Solutions

#### 1. PowerShell Module Installation Issues

**Problem**: `Install-Module` fails or takes too long

```text
Install-Module : Access is denied
```

**Solutions:**

- Run PowerShell as Administrator
- Use `-Scope CurrentUser` if you can't install system-wide
- Check execution policy: `Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass`

#### 2. Azure Authentication Problems

**Problem**: `Connect-AzAccount` fails or wrong subscription

```text
Connect-AzAccount : AADSTS50058: A silent sign-in request was sent but no user is signed in.
```

**Solutions:**

- Ensure you're logged into the correct Azure tenant
- Use `Connect-AzAccount -TenantId <tenant-id>` for specific tenant
- Verify subscription access: `Get-AzSubscription`

#### 3. Event Hub Connection Issues

**Problem**: SQL Server can't connect to Event Hub

```text
Error: Failed to connect to Event Hub endpoint
```

**Solutions:**

- Verify SAS token is not expired (6-month limit)
- Check firewall rules allow outbound connections to `*.servicebus.windows.net`
- Ensure Event Hub namespace and hub names are correct
- Verify the authorization policy has "Send" permissions

#### 4. Change Event Streaming Not Enabled

**Problem**: `sp_enable_event_stream` fails

```text
Msg 40001, Database 'YourDB' is not enabled for change feed
```

**Solutions:**

- Verify SQL Server 2025 version supports CES
- Check database compatibility level
- Ensure you have `db_owner` permissions
- Verify master key exists: `SELECT * FROM sys.symmetric_keys WHERE name = '##MS_DatabaseMasterKey##'`

#### 5. Event Stream Group Creation Fails

**Problem**: Cannot create event stream group

```text
The operation failed because the database scoped credential does not exist
```

**Solutions:**

- Ensure credential was created successfully
- Verify SAS token format (should start with `SharedAccessSignature sr=`)
- Check credential exists: `SELECT * FROM sys.database_scoped_credentials`

#### 6. No Events Being Streamed

**Problem**: Tables added but no events appear in Event Hub

**Solutions:**

- Verify table is added to stream group: `EXEC sp_help_change_feed_table`
- Check stream group status: `SELECT * FROM sys.event_stream_groups`
- Perform DML operations on tracked tables to generate events
- Monitor for errors in SQL Server error log

### Verification Commands

```sql
-- Check if Change Event Streaming is enabled
SELECT name, is_event_stream_enabled 
FROM sys.databases 
WHERE name = DB_NAME();

-- List all event stream groups
SELECT * FROM sys.event_stream_groups;

-- Check tables in stream groups
SELECT * FROM sys.event_stream_group_tables;

-- Verify database scoped credentials
SELECT * FROM sys.database_scoped_credentials;

-- Check stream status for specific table
EXEC sp_help_change_feed_table 
  @source_schema = 'dbo', 
  @source_name = 'EventStore';

-- View all tables with change feed
EXEC sys.sp_help_change_feed_table_groups;
```

## Key Benefits of This Approach

1. **Loose Coupling**: Microservices don't need direct knowledge of each other
2. **Real-time Synchronization**: Changes are streamed immediately
3. **Audit Trail**: Event sourcing provides complete history
4. **Scalability**: Event Hub handles high-throughput scenarios
5. **Reliability**: Built-in retry and checkpoint mechanisms
6. **Standard Protocol**: CloudEvents ensures interoperability

## Resources

- **SQL Server Documentation**: [Change Event Streaming Overview](https://learn.microsoft.com/en-us/sql/relational-databases/track-changes/change-event-streaming/overview?view=sql-server-ver17)
- **Azure Event Hubs**: [Event Hubs Documentation](https://learn.microsoft.com/en-us/azure/event-hubs/)
- **CloudEvents**: [CloudEvents Specification](https://cloudevents.io/)
- **CQRS-ES Pattern**: [Microsoft Architecture Guide](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs)

## Project Structure

```text
WPC-2025/
├── src/
│   ├── BrewUpSales/              # Sales Microservice
│   │   ├── BrewUp.Rest/          # API Layer
│   │   ├── Sales/                # Domain & Infrastructure
│   │   │   ├── BrewUp.Sales.Domain/
│   │   │   ├── BrewUp.Sales.Facade/
│   │   │   └── BrewUp.Sales.Infrastructure/
│   │   ├── BrewUp.Shared/        # Shared Components
│   │   └── Muflone.Persistence.Azure/  # Event Store
│   │
│   └── BrewUpWarehouse/          # Warehouse Microservice
│       ├── BrewUp.Rest/          # API Layer
│       ├── Warehouse/            # Domain & Infrastructure
│       │   ├── BrewUp.Warehouse.Domain/
│       │   ├── BrewUp.Warehouse.Facade/
│       │   └── BrewUp.Warehouse.ReadModel/
│       └── BrewUp.Shared/        # Shared Components
│
├── sqlScripts/                   # SQL Setup Scripts
│   ├── EnableChangeEventStream.sql
│   ├── AddStreamGroupToEventStore.sql
│   ├── AddStreamGroupToProduct.sql
│   ├── Generate-SAS-Token.ps1
│   └── Generate-Warehouse-SAS-Token.ps1
│
└── images/                       # Documentation Images
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Acknowledgments

This demo was created to showcase SQL Server 2025 Change Event Streaming capabilities at WPC 2025.

---

**Note**: This is a demonstration project. For production use, implement proper error handling, monitoring, security practices, and consider using additional patterns like Saga for distributed transactions.
