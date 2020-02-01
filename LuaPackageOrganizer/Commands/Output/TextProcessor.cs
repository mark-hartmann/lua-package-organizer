using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using Pastel;

namespace LuaPackageOrganizer.Commands.Output
{
    public class TextProcessor
    {
        private readonly Dictionary<string, Color> _colorMappings = new Dictionary<string, Color>
        {
            {"package", Color.CornflowerBlue},
            {"release", Color.CornflowerBlue},
            {"warning", Color.Olive},
            {"error", Color.Firebrick},
            {"debug", Color.DarkGray},
            {"file", Color.Coral},
            {"dir", Color.Coral},
        };

        public string Process(string rawMessage)
        {
            var message = string.Empty;
            var document = new XmlDocument();

            document.LoadXml($"<lmsg>{rawMessage}</lmsg>");

            foreach (XmlNode child in document.DocumentElement.ChildNodes)
            {
                switch (child.NodeType)
                {
                    case XmlNodeType.Element:
                    {
                        var containsKey = _colorMappings.ContainsKey(child.Name);
                        var color = _colorMappings[child.Name];

                        message += containsKey ? child.InnerText.Pastel(color) : child.InnerText;
                        break;
                    }
                    case XmlNodeType.Text:
                        message += child.Value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return message;
        }
    }
}