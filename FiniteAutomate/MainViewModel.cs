using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using QuickGraph;
using System.Collections;

namespace FiniteAutomate
{
    /// <summary>
    /// ViewModel of the MainWindow. This is assigned to the MainWindow's DataContext
    /// property. Implements the INotifyPropertyChanged interface to notify the View
    /// of property changes.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private ObservableCollection<FastState> states;

        private int start = 0;

        public int itemCount = 25;

        private string sortColumn = "Name";

        private bool ascending = true;

        private int totalItems = 0;

        private ICommand firstCommand;

        private ICommand previousCommand;

        private ICommand nextCommand;

        private ICommand lastCommand;

        private static ObservableCollection<FastState> all_states = new ObservableCollection<FastState>();

        public string PageNumber
        {
            get
            {
                return (start / itemCount + 1).ToString();
            }
            set
            {
                start = (int.Parse(value) - 1) * itemCount;
                RefreshStates();
            }
        }

        /// <summary>
        /// Constructor. Initializes the list of states.
        /// </summary>
        public MainViewModel()
        {
            RefreshStates();
        }

        /// <summary>
        /// The list of products in the current page.
        /// </summary>
        public ObservableCollection<FastState> States
        {
            get
            {
                return states;
            }
            private set
            {
                if (object.ReferenceEquals(states, value) != true)
                {
                    states = value;
                    NotifyPropertyChanged("States");
                }
            }
        }

        /// <summary>
        /// Gets the index of the first item in the products list.
        /// </summary>
        public int Start { get { return start + 1; } }

        /// <summary>
        /// Gets the index of the last item in the products list.
        /// </summary>
        public int End { get { return start + itemCount < totalItems ? start + itemCount : totalItems ; } }

        /// <summary>
        /// The number of total items in the data store.
        /// </summary>
        public int TotalItems { get { return totalItems; } }

        /// <summary>
        /// Gets the command for moving to the first page of products.
        /// </summary>
        public ICommand FirstCommand
        {
            get
            {
                if (firstCommand == null)
                {
                    firstCommand = new RelayCommand
                    (
                        param =>
                        {
                            start = 0;
                            RefreshStates();
                        },
                        param =>
                        {
                            return start - itemCount >= 0 ? true : false;
                        }
                    );
                }

                return firstCommand;
            }
        }

        /// <summary>
        /// Gets the command for moving to the previous page of products.
        /// </summary>
        public ICommand PreviousCommand
        {
            get
            {
                if (previousCommand == null)
                {
                    previousCommand = new RelayCommand
                    (
                        param =>
                        {
                            start -= itemCount;
                            RefreshStates();
                        },
                        param =>
                        {
                            return start - itemCount >= 0 ? true : false;
                        }
                    );
                }

                return previousCommand;
            }
        }

        /// <summary>
        /// Gets the command for moving to the next page of products.
        /// </summary>
        public ICommand NextCommand
        {
            get
            {
                if (nextCommand == null)
                {
                    nextCommand = new RelayCommand
                    (
                        param =>
                        {
                            start += itemCount;
                            RefreshStates();
                        },
                        param =>
                        {
                            return start + itemCount < totalItems ? true : false;
                        }
                    );
                }

                return nextCommand;
            }
        }

        /// <summary>
        /// Gets the command for moving to the last page of products.
        /// </summary>
        public ICommand LastCommand
        {
            get
            {
                if (lastCommand == null)
                {
                    lastCommand = new RelayCommand
                    (
                        param =>
                        {
                            start = (totalItems / itemCount - 1) * itemCount;
                            start += totalItems % itemCount == 0 ? 0 : itemCount;
                            RefreshStates();
                        },
                        param =>
                        {
                            return start + itemCount < totalItems ? true : false;
                        }
                    );
                }

                return lastCommand;
            }
        }

        /// <summary>
        /// Sorts the list of products.
        /// </summary>
        /// <param name="sortColumn">The column or member that is the basis for sorting.</param>
        /// <param name="ascending">Set to true if the sort</param>
        public void Sort(string sortColumn, bool ascending)
        {
            this.sortColumn = sortColumn;
            this.ascending = ascending;

            RefreshStates();
        }

        /// <summary>
        /// Refreshes the list of products. Called by navigation commands.
        /// </summary>
        private void RefreshStates()
        {
            States = GetStates(start, itemCount, sortColumn, ascending, out totalItems);

            NotifyPropertyChanged("Start");
            NotifyPropertyChanged("End");
            NotifyPropertyChanged("TotalItems");
            NotifyPropertyChanged("PageNumber");
        }

        /// <summary>
        /// Notifies subscribers of changed properties.
        /// </summary>
        /// <param name="propertyName">Name of the changed property.</param>
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Gets the states.
        /// </summary>
        /// <param name="start">Zero-based index that determines the start of the states to be returned.</param>
        /// <param name="itemCount">Number of states that is requested to be returned.</param>
        /// <param name="sortColumn">Name of column or member that is the basis for sorting.</param>
        /// <param name="ascending">Indicates the sort direction to be used.</param>
        /// <param name="totalItems">Total number of states.</param>
        /// <returns>List of states.</returns>
        public ObservableCollection<FastState> GetStates(int start, int itemCount, string sortColumn, bool ascending, out int totalItems)
        {
            totalItems = all_states.Count;
            if (totalItems == 0) return all_states;

            ObservableCollection<FastState> sortedStates = new ObservableCollection<FastState>();

            // Sort the products
            if (sortColumn == "Name")
            {
                sortedStates = new ObservableCollection<FastState>
                (
                    from p in all_states
                    orderby p.Id
                    select p
                );
            }
            else
            {
                int ind1 = sortColumn.IndexOf('(');
                int ind2 = sortColumn.IndexOf(')');
                int field_number = int.Parse(sortColumn.Substring(ind1 + 1, ind2 - ind1 - 1));
                sortedStates = new ObservableCollection<FastState>
                (
                    from p in all_states
                    orderby p.NextState(field_number)
                    select p
                );
            }

            sortedStates = ascending ? sortedStates : new ObservableCollection<FastState>(sortedStates.Reverse());

            ObservableCollection<FastState> filteredStates = new ObservableCollection<FastState>();

            for (int i = start; i < start + itemCount && i < totalItems; i++)
            {
                filteredStates.Add(sortedStates[i]);
                //filteredProducts.Add(all_states[i]);
            }

            return filteredStates;
        }

        //public void BuildStates(List<string> InputChars, AdjacencyGraph<VertexExp, TaggedEdge<VertexExp, string>> g)
/*
        public void BuildStates(List<string> InputChars, BidirectionalGraph<object, IEdge<object>> g)
        {
            all_states.Clear();

            foreach (var v in g.Vertices)
            {
                all_states.Add(new State(InputChars, g, v));
            }

            RefreshStates();
        }
*/
        public void BuildStates(Hashtable FastGraph, int InputCharsSize)
        {
            all_states.Clear();

            foreach (DictionaryEntry de in FastGraph)
            {
                FastState fs = de.Value as FastState;
                //fs.NextStateDescr = new string[InputCharsSize];
                //for (int i = 0; i < InputCharsSize; i++) fs.NextStateDescr[i] = fs.CalcNextStateDescr(i);
                all_states.Add(fs);
            }

            RefreshStates();
        }
    }
}
