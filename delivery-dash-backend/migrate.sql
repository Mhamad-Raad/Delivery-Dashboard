START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260423151251_AddOrderStatusTrackingAndConcurrency') THEN
    DROP INDEX "IX_OrderAssignments_OrderId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260423151251_AddOrderStatusTrackingAndConcurrency') THEN
    ALTER TABLE "Orders" ADD "CancelledAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260423151251_AddOrderStatusTrackingAndConcurrency') THEN
    ALTER TABLE "Orders" ADD "ConfirmedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260423151251_AddOrderStatusTrackingAndConcurrency') THEN
    ALTER TABLE "Orders" ADD "DeliveredAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260423151251_AddOrderStatusTrackingAndConcurrency') THEN
    ALTER TABLE "Orders" ADD "OutForDeliveryAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260423151251_AddOrderStatusTrackingAndConcurrency') THEN
    ALTER TABLE "Orders" ADD "PreparingAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260423151251_AddOrderStatusTrackingAndConcurrency') THEN
    CREATE INDEX "IX_Orders_DeliveredAt" ON "Orders" ("DeliveredAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260423151251_AddOrderStatusTrackingAndConcurrency') THEN
    CREATE UNIQUE INDEX "UX_OrderAssignments_OrderId_Accepted" ON "OrderAssignments" ("OrderId") WHERE "Status" = 1;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260423151251_AddOrderStatusTrackingAndConcurrency') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260423151251_AddOrderStatusTrackingAndConcurrency', '9.0.10');
    END IF;
END $EF$;
COMMIT;

