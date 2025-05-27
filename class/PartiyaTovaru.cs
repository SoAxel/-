using System;

public class PartiyaTovaru
{
    public Gorodyna Gorodyna { get; set; }
    public DeliveryMethod Delivery { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TransportCost { get; set; }
    public DateTime DeliveryDate { get; set; }

    public decimal TotalCost => Quantity * UnitPrice + TransportCost;

    public override string ToString() =>
        $"{Gorodyna} - {Quantity} шт. x {UnitPrice} грн, Тр: {TransportCost} грн, {Delivery}, {DeliveryDate:d}";
}
