using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiModels
{
    public class MinesGameApiModel
    {
        public string Cells { get; set; }

        public int OpenedCount { get; set; }

        public int MinesCount { get; set; }

        public decimal BetSum { get; set; }
    }

    public class CellApiModel
    {
        [DataMember]
        public int X { get; set; }
        [DataMember]
        public int Y { get; set; }
        [DataMember]
        public bool IsOpen { get; set; }

        public CellApiModel(int x, int y, bool isOpen)
        {
            X = x;
            Y = y;
            IsOpen = isOpen;
        }
    }

    public class CellApiModelArrayConverter : JsonConverter<CellApiModel[,]>
    {
        public override CellApiModel[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, CellApiModel[,] value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            for (int i = 0; i < value.GetLength(0); i++)
            {
                writer.WriteStartArray();

                for (int j = 0; j < value.GetLength(1); j++)
                {
                    var cell = value[i, j];

                    writer.WriteStartObject();
                    writer.WriteNumber("X", cell.X);
                    writer.WriteNumber("Y", cell.Y);
                    writer.WriteBoolean("IsOpen", cell.IsOpen);
                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }

    }

    public class CellApiArrayConverter : JsonConverter<Cell[,]>
    {
        public override Cell[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Cell[,] value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            for (int i = 0; i < value.GetLength(0); i++)
            {
                writer.WriteStartArray();

                for (int j = 0; j < value.GetLength(1); j++)
                {
                    var cell = value[i, j];

                    writer.WriteStartObject();
                    writer.WriteNumber("X", cell.X);
                    writer.WriteNumber("Y", cell.Y);
                    writer.WriteBoolean("IsOpen", cell.IsOpen);
                    writer.WriteBoolean("IsMined", cell.IsMined);

                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }

    }
}
