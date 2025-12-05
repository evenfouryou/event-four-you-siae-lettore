using System;
using System.Text;
using Newtonsoft.Json;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        var reader = new SIAEReader();

        SendMessage(new {
            command = "BRIDGE_READY",
            success = true,
            message = "Bridge avviato"
        });

        while (true)
        {
            try
            {
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                    continue;

                dynamic cmd = JsonConvert.DeserializeObject(input);
                string command = cmd.command;

                switch (command)
                {
                    case "CHECK_READER":
                        HandleCheckReader(reader);
                        break;

                    case "READ_ATR":
                        HandleReadATR(reader);
                        break;

                    default:
                        SendMessage(new {
                            success = false,
                            message = "Comando sconosciuto"
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                SendMessage(new {
                    success = false,
                    message = "Errore interno",
                    error = ex.Message
                });
            }
        }
    }

    static void HandleCheckReader(SIAEReader reader)
    {
        bool connected = reader.IsReaderConnected();
        bool cardPresent = reader.IsCardPresent();

        SendMessage(new {
            command = "CHECK_READER",
            success = true,
            readerConnected = connected,
            cardPresent = cardPresent,
            message = connected
                ? (cardPresent ? "Carta presente" : "Lettore connesso - inserire la carta SIAE")
                : "Lettore non rilevato"
        });
    }

    static void HandleReadATR(SIAEReader reader)
    {
        if (!reader.IsReaderConnected())
        {
            SendMessage(new {
                command = "READ_ATR",
                success = false,
                message = "Lettore non rilevato"
            });
            return;
        }

        if (!reader.IsCardPresent())
        {
            SendMessage(new {
                command = "READ_ATR",
                success = false,
                message = "Carta NON rilevata"
            });
            return;
        }

        var atr = reader.GetATR();
        if (atr == null || atr.Length == 0)
        {
            SendMessage(new {
                command = "READ_ATR",
                success = false,
                message = "Impossibile leggere ATR"
            });
        }
        else
        {
            string atrHex = BitConverter.ToString(atr).Replace("-", " ");

            SendMessage(new {
                command = "READ_ATR",
                success = true,
                atr = atrHex,
                message = "ATR letto con successo"
            });
        }
    }

    static void SendMessage(object obj)
    {
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        Console.WriteLine(json);
        Console.Out.Flush();
    }
}
