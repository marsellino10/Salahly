CREATE PROCEDURE SP_CleanupUnpaidBookings
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @AffectedBookings INT = 0;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Get IDs of bookings that need cleanup
        DECLARE @BookingsToCleanup TABLE (BookingId INT, ServiceRequestId INT);
        
        -- Find bookings with expired deadline (and not already marked as Unpaid)
        INSERT INTO @BookingsToCleanup (BookingId, ServiceRequestId)
        SELECT 
            b.BookingId,
            b.ServiceRequestId
        FROM Bookings b
        WHERE 
            b.PaymentDeadline < GETUTCDATE()
            AND b.Status = 1;  -- pending
        
        SET @AffectedBookings = @@ROWCOUNT;
        
        -- Step 1: Update Booking Status to Unpaid
        UPDATE b
        SET 
            b.Status = 5,  -- Unpaid
            b.UpdatedAt = GETUTCDATE()
        FROM Bookings b
        INNER JOIN @BookingsToCleanup btc ON b.BookingId = btc.BookingId;
        
        -- Step 2: Update Pending Payments to Failed
        UPDATE p
        SET 
            p.Status = 2,  -- Failed
            p.FailureReason = 'Payment deadline expired'
        FROM Payments p
        INNER JOIN @BookingsToCleanup btc ON p.BookingId = btc.BookingId
        WHERE p.Status = 0;  -- Only Pending payments
        
        -- Step 3: Reset ALL CraftsmanOffers for these ServiceRequests to Pending
        UPDATE co
        SET 
            co.Status = 0,  -- Pending
            co.UpdatedAt = GETUTCDATE(),
            co.AcceptedAt = NULL
        FROM CraftsmanOffers co
        INNER JOIN @BookingsToCleanup btc ON co.ServiceRequestId = btc.ServiceRequestId;
        
        COMMIT TRANSACTION;
        
        -- Return affected count
        SELECT @AffectedBookings AS AffectedCount;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        -- Re-throw error
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO