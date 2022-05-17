using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mdl;

namespace mdl_winform {

    /// <summary>
    /// Interface for managing form events
    /// </summary>
    public interface IFormEventsManager : IDisposable {
        /// <summary>
        /// True if autoevents are enabled (i.e. all automatic actions called when
        ///  a current row selected in control like combobox, grid, tree changes
        /// </summary>
        bool AutoEventEnabled { get; }

        /// <summary>
        /// (Re)Enable Automatic Events (i.e. ControlChanged)
        /// </summary>
        void EnableAutoEvents();

        /// <summary>
        /// Disable Automatic Events, i.e. ControlChanged Events 
        /// </summary>
        void DisableAutoEvents();

        /// <summary>
        ///  Adds a listener to a specified event
        /// </summary>
        /// <param name="handler"></param>
        /// <typeparam name="TEvent"></typeparam>
        void addListener<TEvent>(ApplicationEventHandlerDelegate<TEvent> handler) where TEvent : IApplicationEvent;

        /// <summary>
        /// Removes a listener from a specified event
        /// </summary>
        /// <param name="handler"></param>
        /// <typeparam name="TEvent"></typeparam>
        void removeListener<TEvent>(ApplicationEventHandlerDelegate<TEvent> handler) where TEvent : IApplicationEvent;


        /// <summary>
        /// Fires an event unless events have been disabled
        /// </summary>
        /// <param name="event"></param>
        /// <typeparam name="TEvent"></typeparam>
        void dispatch<TEvent>(TEvent @event) where TEvent : IApplicationEvent;
    }

    /// <summary>
    /// Implementations of a IFormEventsManager
    /// </summary>
    public class FormEventsManager : IFormEventsManager {

        
        private int _myAutoEventEnabled ;
        void changeAutoEvent(bool enabledisable) {
            if (enabledisable) {
                //enable:
                _myAutoEventEnabled--;
                //if (_myAutoEventEnabled < 0) MarkEvent("myAutoEvent < 0");
            }
            else {
                _myAutoEventEnabled++;
            }
          
        }

        /// <summary>
        /// True if autoevents are enabled (i.e. all automatic actions called when
        ///  a current row selected in control like combobox, grid, tree changes
        /// </summary>
        public bool AutoEventEnabled => (_myAutoEventEnabled == 0);

        /// <summary>
        /// (Re)Enable Automatic Events (i.e. ControlChanged)
        /// </summary>
        public void EnableAutoEvents() {
            changeAutoEvent(true);
        }

        /// <summary>
        /// Disable Automatic Events, i.e. ControlChanged Events 
        /// </summary>
        public void DisableAutoEvents() {
            changeAutoEvent(false);
        }

        private bool _disposed;
        
        /// <summary>
        /// destructor
        /// </summary>
        ~FormEventsManager() {
            Dispose(false);
        }

        /// <summary>
        /// Implement IDispose
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing) {
            if (_disposed) return;

            if (disposing) {
                // free other managed objects that implement IDisposable only
            }

            // release any unmanaged objects
            // set the object references to null
            removeAllListeners();
            _eventHandlers = null;


            _disposed = true;
        }


        private void removeAllListeners() {
            var handlerTypes = new Type[_eventHandlers.Keys.Count];
            _eventHandlers.Keys.CopyTo(handlerTypes, 0);

            foreach (Type handlerType in handlerTypes) {
                Delegate[] delegates = _eventHandlers[handlerType].GetInvocationList();
                foreach (Delegate @delegate1 in delegates) {
                    var handlerToRemove = Delegate.Remove(_eventHandlers[handlerType], @delegate1);
                    if (handlerToRemove == null) {
                        _eventHandlers.Remove(handlerType);
                    }
                    else {
                        _eventHandlers[handlerType] = handlerToRemove;
                    }
                }
            }
        }

        private Dictionary<Type, Delegate> _eventHandlers;

        /// <summary>
        /// 
        /// </summary>
        public FormEventsManager() {
            _eventHandlers = new Dictionary<Type, Delegate>();
        }

        /// <inheritdoc />
        public virtual void addListener<TEvent>(ApplicationEventHandlerDelegate<TEvent> handler)
            where TEvent : IApplicationEvent {
            Delegate @delegate;
            if (_eventHandlers.TryGetValue(typeof(TEvent), out @delegate)) {
                _eventHandlers[typeof(TEvent)] = Delegate.Combine(@delegate, handler);
            }
            else {
                _eventHandlers[typeof(TEvent)] = handler;
            }
        }

        /// <inheritdoc />
        public virtual void removeListener<TEvent>(ApplicationEventHandlerDelegate<TEvent> handler)
            where TEvent : IApplicationEvent {
            Delegate @delegate;
            if (!_eventHandlers.TryGetValue(typeof(TEvent), out @delegate)) return;
            var currentDel = Delegate.Remove(@delegate, handler);

            if (currentDel == null) {
                _eventHandlers.Remove(typeof(TEvent));
            }
            else {
                _eventHandlers[typeof(TEvent)] = currentDel;
            }
        }

        /// <inheritdoc />
        public virtual void dispatch<TEvent>(TEvent @event) where TEvent : IApplicationEvent {
            if (!AutoEventEnabled)return;
            
            if (@event == null) {
                throw new ArgumentNullException(nameof(@event));
            }

            Delegate @delegate;
            if (_eventHandlers.TryGetValue(typeof(TEvent), out @delegate)) {
                (@delegate as ApplicationEventHandlerDelegate<TEvent>)?.Invoke(@event);
            }
        }

    }
}
