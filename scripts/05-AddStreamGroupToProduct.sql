EXEC sys.sp_create_event_stream_group
  @stream_group_name      = 'wh-ces-2025',
  @destination_location   = 'wpc-eventhub.servicebus.windows.net/producthub',
  @destination_credential = WarehouseCredential,
  @destination_type       = 'AzureEventHubsAmqp'

EXEC sys.sp_add_object_to_event_stream_group
  @stream_group_name      = 'wh-ces-2025',
  @object_name = 'dbo.Product',
  @include_old_values = 1,
  @include_all_columns = 1

EXEC sp_help_change_feed_table @source_schema = 'dbo', @source_name = 'Product'

EXEC sys.sp_help_change_feed_table_groups;
EXEC sys.sp_help_change_feed_table @source_schema = 'dbo', @source_name = 'Product';