using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;

namespace TextProcessor
{
    public class MessageProcessor
    {
        private readonly Dictionary<string, Func<string, string>> _customNodes =
            new Dictionary<string, Func<string, string>>();

        private readonly Dictionary<string, Func<string>> _customSelfClosingNodes =
            new Dictionary<string, Func<string>>();

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
                if (child.NodeType == XmlNodeType.Element)
                {
                    var childElement = (XmlElement) child;

                    if (childElement.IsEmpty == false && _customNodes.ContainsKey(childElement.Name))
                    {
                        var innerText = Process(child.InnerXml);
                        message += _customNodes[childElement.Name](innerText);
                    }

                    if (childElement.IsEmpty && _customSelfClosingNodes.ContainsKey(childElement.Name))
                        message += _customSelfClosingNodes[childElement.Name]();
                }
                else if (child.NodeType == XmlNodeType.Text)
                {
                    message += child.Value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            return message;
        }

        public void AddCustomNode(string name, Func<string> func) => _customSelfClosingNodes[name] = func;

        public void AddCustomNode(string name, Func<string, string> func) => _customNodes[name] = func;
    }
}