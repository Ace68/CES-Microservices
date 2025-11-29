EXEC sys.sp_create_event_stream_group
  @stream_group_name      = 'ces-wpc-2025',
  @destination_location   = 'wpc-eventhub.servicebus.windows.net/eventstorehub',
  @destination_credential = SqlCesCredential,
  @destination_type       = 'AzureEventHubsAmqp'


EXEC sys.sp_create_event_stream_group
  @stream_group_name      = 'warehouse-ces-2025',
  @destination_location   = 'wpc-eventhub.servicebus.windows.net/producthub',
  @destination_credential = WarehouseCredential,
  @destination_type       = 'AzureEventHubsAmqp'