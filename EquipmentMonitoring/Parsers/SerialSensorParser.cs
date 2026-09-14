using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace EquipmentMonitoring.Parsers;

public class SerialSensorData
{
    public double Temperature { get; set; }
    public double Pressure { get; set; }
}

public class SerialSensorParser
{
    public bool TryParse(
        string packet,
        out SerialSensorData? data)
    {
        data = null;

        if (string.IsNullOrWhiteSpace(packet))
            return false;

        string[] fields =
            packet.Split(',');

        double? temperature = null;
        double? pressure = null;

        foreach (string field in fields)
        {
            string[] pair =
                field.Split(':');

            if (pair.Length != 2)
                return false;

            string key =
                pair[0].Trim();

            string value =
                pair[1].Trim();

            if (key.Equals(
                "TEMP",
                StringComparison.OrdinalIgnoreCase))
            {
                if (!double.TryParse(
                    value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double temp))
                {
                    return false;
                }

                temperature = temp;
            }

            else if (key.Equals(
                "PRESSURE",
                StringComparison.OrdinalIgnoreCase))
            {
                if (!double.TryParse(
                    value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double press))
                {
                    return false;
                }

                pressure = press;
            }
        }

        if (temperature == null ||
            pressure == null)
        {
            return false;
        }

        data = new SerialSensorData
        {
            Temperature =
                temperature.Value,

            Pressure =
                pressure.Value,
        };

        return true;
    }
}
