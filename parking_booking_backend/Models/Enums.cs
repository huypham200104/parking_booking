namespace parking_booking_backend.Models
{
    public enum Role
    {
        Driver,
        ParkingOwner,
        Guard,
        Admin
    }

    public enum VehicleType
    {
        Sedan,
        SUV,
        Hatchback,
        Motorbike,
        Other
    }

    public enum SlotVehicleType
    {
        Car,
        Bike
    }

    public enum ParkingLotStatus
    {
        PendingApproval,
        Active,
        Suspended
    }

    public enum ParkingSlotStatus
    {
        Available,
        Occupied,
        Maintenance
    }

    public enum BookingStatus
    {
        Pending,
        CheckedIn,
        Completed,
        Cancelled,
        NoShow
    }

    public enum PaymentMethod
    {
        VietQR,
        Cash,
        Momo,
        Wallet
    }

    public enum TransactionStatus
    {
        Pending,
        Success,
        Failed
    }

    public enum ReportStatus
    {
        Full,
        Available
    }

    public enum MonthlyPassStatus
    {
        Active,
        Expired,
        Cancelled,
        PendingPayment
    }

    public enum WalletTransactionType
    {
        Deposit,
        Withdraw,
        Payment
    }
}
