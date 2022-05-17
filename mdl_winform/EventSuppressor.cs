using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mdl_winform {
    /// <summary>
    /// Helper class to suppress events
    /// </summary>
    public class EventSuppressor {

        /// <summary>
        /// Suppresses events linked to a control
        /// </summary>
        /// <param name="control"></param>
        public static void EventSuppress(Control control) {

            if (control == null)
                throw new ArgumentNullException("control", "An instance of a control must be provided.");

            var _sourceEventsInfo = control.GetType().GetProperty("Events", BindingFlags.Instance | BindingFlags.NonPublic);
            var _sourceEventHandlerList = (EventHandlerList)_sourceEventsInfo.GetValue(control, null);
            var _eventHandlerListType = _sourceEventHandlerList.GetType();
            var _headFI = _eventHandlerListType.GetField("head", BindingFlags.Instance | BindingFlags.NonPublic);
            if (_headFI == null) return;
            var _handlers = buildList(_headFI, _sourceEventHandlerList);

            foreach (var pair in _handlers) {
                for (int x = pair.Value.Length - 1; x >= 0; x--) {
                    if (pair.Value[x].Method.Name == "HandleDestroyed") continue;
                    if (pair.Value[x].Method.Name == "HandleCreated") continue;
                    _sourceEventHandlerList.RemoveHandler(pair.Key, pair.Value[x]);
                }

            }

        }

        private static Dictionary<object, Delegate[]> buildList(FieldInfo _headFI, EventHandlerList _sourceEventHandlerList) {
            var _handlers = new Dictionary<object, Delegate[]>();
            object head = _headFI.GetValue(_sourceEventHandlerList);
            if (head != null) {
                var listEntryType = head.GetType();
                var delegateFI = listEntryType.GetField("handler", BindingFlags.Instance | BindingFlags.NonPublic);
                var keyFI = listEntryType.GetField("key", BindingFlags.Instance | BindingFlags.NonPublic);
                var nextFI = listEntryType.GetField("next", BindingFlags.Instance | BindingFlags.NonPublic);
                buildListWalk(_handlers, head, delegateFI, keyFI, nextFI);
            }
            return _handlers;
        }

        private static void buildListWalk(Dictionary<object, Delegate[]> _handlers,
            object entry, FieldInfo delegateFI, FieldInfo keyFI, FieldInfo nextFI) {
            if (entry != null) {
                var dele = (Delegate)delegateFI.GetValue(entry);
                if (dele == null) return;
                object key = keyFI.GetValue(entry);
                object next = nextFI.GetValue(entry);

                var listeners = dele.GetInvocationList();

                if (listeners != null && listeners.Length > 0)
                    _handlers.Add(key, listeners);

                if (next != null) {
                    buildListWalk(_handlers, next, delegateFI, keyFI, nextFI);
                }
            }
        }


    }
}
