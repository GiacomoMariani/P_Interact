using System.Collections.Generic;
using JReact.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;

namespace JReact.Playfab_Interact.Data
{
    /// <summary>
    /// the interaction tasks used to manage playfab data
    /// </summary>
    public abstract class P_InteractionTasks<T> : ScriptableObject, iResettable
        where T : P_SaveableTask, new()
    {
        #region TASK ELEMENTS
        [BoxGroup("Task", true, true, -25), SerializeField, AssetsOnly, Required] private J_TaskQueue _playfabInteractionQueue;
        #endregion

        #region VALUES AND PROPERTIES
        //the max amount of elements, currently (oct 2018) playfab accept at most 10 elements
        [BoxGroup("Setup", true, true, 0), SerializeField, Range(1, P_Constants.MaxRequestAmount)]
        private int _maxElementsToSend = 10;
        //max task to send
        [BoxGroup("Setup", true, true, 0), SerializeField, Range(1, 20)]
        private int _allocatedTask = 10;
        //the list of elements to be saved, we can have different groups by instantiating different scriptable objects
        [BoxGroup("Setup", true, true, 0), SerializeField, AssetsOnly, Required] protected P_Saveable[] _groupOfElements;
        //the transfer control that will send the data to playfab
        [BoxGroup("Setup", true, true, 0), SerializeField, AssetsOnly, Required]
        private P_PlayfabDataTransfer _dataTransfer;

        //initialization
        [BoxGroup("State", true, true, 5), ShowInInspector, ReadOnly] private bool _initialized = false;
        //the queue of saving
        [BoxGroup("State", true, true, 5), ShowInInspector, ReadOnly]
        protected Queue<P_Saveable> _saveableInQueue = new Queue<P_Saveable>();
        //the current tasks
        [BoxGroup("State", true, true, 5), ShowInInspector, ReadOnly] private T[] _tasks;
        //the current task
        [BoxGroup("State", true, true, 5), ShowInInspector, ReadOnly] protected T _CurrentTask
        {
            get
            {
                if (_tasks == null) return null;
                Assert.IsTrue(NextTask < _tasks.Length,
                              string.Format("{0} requested task {1}. But there are only  task {2}", name, NextTask, _tasks.Length));
                return _tasks[NextTask];
            }
        }
        //current task
        [BoxGroup("State", true, true, 5), ShowInInspector, ReadOnly] private int _nextTask = 0;
        public int NextTask { get { return _nextTask; } protected set { _nextTask = value % _allocatedTask; } }
        #endregion

        #region ABSTRACT IMPLEMENTATION
        protected abstract void SpecificInitialization();
        protected abstract void SetupCurrentTask(int maxElements);
        #endregion

        #region INITIATION
        /// <summary>
        /// this is used to initiate the save loop. It register all the containing elements, so they can talk to each other
        /// </summary>
        public void InitiateTasks()
        {
            if (_initialized) return;
            HelperConsole
                .DisplayMessage(string.Format("{0} (Save Group) Initiating with {1} elements.", name, _groupOfElements.Length),
                                P_Constants.DEBUG_PlayfabInteractImportant);
            //make sure this is ready to go
            SanityChecks();
            //register all elements
            SpecificInitialization();
            //setup the tasks and the arrays
            SetupTasks(_allocatedTask);
            //set the base task
            NextTask = 0;
            //confirm initialization
            _initialized = true;
        }

        //setting up the task for the pool
        private void SetupTasks(int maxTasks)
        {
            _tasks = new T[maxTasks];
            for (int i = 0; i < maxTasks; i++)
            {
                var newTask = new T();
                newTask.InjectData(_dataTransfer, i);
                _tasks[i] = newTask;
            }
        }

        //makes sure everything is ready
        private void SanityChecks()
        {
            //make sure we have at least one element
            Assert.IsTrue(_groupOfElements.Length > 0, string.Format("{0} The save queue requires at least one element.", name));
            Assert.IsNotNull(_playfabInteractionQueue, string.Format("({0}) requires a _playfabInteractionQueue", name));

            //make sure the queue is not more than possible
            Assert.IsTrue(_maxElementsToSend <= P_Constants.MaxRequestAmount,
                          string.Format("{0}{1} queue has {2} as max queue to be saved, while the max amount is {3}",
                                        P_Constants.DEBUG_PlayfabInteract, name, _maxElementsToSend,
                                        P_Constants.MaxRequestAmount));
        }
        #endregion

        #region CHECKS
        /// checks if a saveable is scheduled to be processed
        private bool IsThisScheduled(P_Saveable saveable) { return _saveableInQueue.Contains(saveable); }
        #endregion

        #region HELPERS
        /// <summary>
        /// this is the main command to process these tasks. it stores the desired saveable into a queue.
        /// </summary>
        internal void ScheduleThis(P_Saveable saveable)
        {
            //ignore if this is saving already
            if (IsThisScheduled(saveable))
            {
                P_PlayfabConsoleLogger.DisplayMessage(string.Format("Interaction canceled. This is already in queue {0}",
                                                                    saveable.name), name);
                return;
            }

            _saveableInQueue.Enqueue(saveable);
        }


        //used to create a chunk of elements to save
        protected P_Saveable[] GetArraySegment(int maxElementsToSend)
        {
            //find how many things we want to send
            int saveablesTotal                                          = maxElementsToSend;
            if (_saveableInQueue.Count < saveablesTotal) saveablesTotal = _saveableInQueue.Count;
            //setup the array
            var _saveables = new P_Saveable[saveablesTotal];
            for (int i = 0; i < saveablesTotal; i++)
                _saveables[i] = _saveableInQueue.Dequeue();

            return _saveables;
        }
        #endregion

        #region SAVE COMMANDS
        /// <summary>
        /// process all
        /// </summary>
        public void ForceSaveAll() { ProcessTrunk(_groupOfElements); }

        /// <summary>
        /// process a trunk of elements
        /// </summary>
        /// <param name="elementsToSave">the elements to be processed</param>
        public void ProcessTrunk(P_Saveable[] elementsToSave)
        {
            Assert.IsNotNull(elementsToSave, string.Format("({0}) needs an element for elementsToSave", name));
            Assert.IsTrue(elementsToSave.Length > 0, string.Format("({0}) requires at least one element to save", name));
            HelperConsole.DisplayMessage(string.Format("{0} Schedule save force for {1} elements.", name, elementsToSave.Length),
                                         P_Constants.DEBUG_PlayfabInteractImportant);

            //enqueue all elements, then process them
            for (int i = 0; i < elementsToSave.Length; i++)
                ScheduleThis(elementsToSave[i]);

            ProcessCurrentQueue();
        }

        /// <summary>
        /// used to process the current queue
        /// </summary>
        [Button("Save Data", ButtonSizes.Medium), BoxGroup("Test", true, true, 100)]
        public void ProcessCurrentQueue()
        {
            //ignore empty queue
            if (_saveableInQueue.Count == 0) return;
            //initialize if required
            if (!_initialized) InitiateTasks();
            P_PlayfabConsoleLogger.DisplayMessage(string.Format
                                                      ("Sending a task. Queue remaining data {0}", _saveableInQueue.Count), name);

            //we request to save this enough time to save the current saveables
            int totalTaskRequired = (_saveableInQueue.Count / _maxElementsToSend) + 1;
            for (int i = 0; i < totalTaskRequired; i++)
            {
                //make sure the task is not running
                Assert.IsFalse(_CurrentTask.IsRunning,
                               string.Format("{0} is trying to access {1}, that is running already", name, _CurrentTask.TaskName));

                //MAIN - setup the task and process it
                SetupCurrentTask(_maxElementsToSend);
                P_PlayfabConsoleLogger.DisplayMessage(string.Format("Setup the task {0}", _CurrentTask.TaskName), name);
                _playfabInteractionQueue.ProcessTask(_CurrentTask);
                //update the task
                NextTask++;
            }
        }
        #endregion

        #region DISABLE RESET
        //reset this on disable
        private void OnDisable() { ResetThis(); }

        public virtual void ResetThis()
        {
            _saveableInQueue.Clear();
            _initialized = false;
        }
        #endregion
    }
}
