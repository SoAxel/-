using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;

public class Sklad
{
    public int Number { get; set; }
    public decimal MaintenanceCost { get; set; }
    public List<PartiyaTovaru> Partiyi { get; set; } = new List<PartiyaTovaru>();

    public override string ToString() => $"Склад #{Number}, Обслуговування: {MaintenanceCost} грн, Партій: {Partiyi.Count}";
    public decimal TotalValue => Partiyi.Sum(p => p.TotalCost);

}

