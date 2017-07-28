using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;
using System.Xml;
using Microsoft.Win32;
using QuickGraph;
using QuickGraph.Algorithms;
using Telerik.Windows.Controls;
using System.ComponentModel;
using GraphSharp.Controls;
using System.Collections.Specialized;
using System.Collections;

namespace FiniteAutomate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DataGridColumn currentSortColumn;

        private ListSortDirection currentSortDirection;

        string FileNameDump = "";
        string FileNameXML = "";
        List<string> InputChars = new List<string>();
        Hashtable FastGraph = new Hashtable(); 
        //AdjacencyGraph<VertexExp, TaggedEdge<VertexExp, string>> g = null;
        BidirectionalGraph<object, IEdge<object>> g;
        MainViewModel mainViewModel = null;

        BidirectionalGraph<object, IEdge<object>> part;

        DispatcherTimer myClickWaitTimer;
        bool IsSingleClick = true;

        bool GraphLayoutBuilded = false;
        bool IsFullScreen = false;
        int SelVertInGrid = -1;

        string layoutAlgorithmType = "Tree";
        public string LayoutAlgorithmType {
            get { return layoutAlgorithmType; }
            set 
            { 
                layoutAlgorithmType = value;
                OnPropertyChanged("LayoutAlgorithmType");
            } 
        }

        AlgorithmConstraints edgeRoutingConstraint = AlgorithmConstraints.Must;
        public AlgorithmConstraints EdgeRoutingConstraint {
            get { return edgeRoutingConstraint; }
            set { edgeRoutingConstraint = value; }
        }

        AlgorithmConstraints overlapRemovalConstraint = AlgorithmConstraints.Must;
        public AlgorithmConstraints OverlapRemovalConstraint {
            get { return overlapRemovalConstraint; }
            set { overlapRemovalConstraint = value; }
        }

        string overlapRemovalAlgorithmType = "FSA";
        public string OverlapRemovalAlgorithmType {
            get { return overlapRemovalAlgorithmType; }
            set { overlapRemovalAlgorithmType = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            mainViewModel = new MainViewModel();
            DataContext = mainViewModel;
            mainViewModel.PropertyChanged +=new PropertyChangedEventHandler(mainViewModel_PropertyChanged);
            myClickWaitTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 500), DispatcherPriority.Background, mouseWaitTimer_Tick, Dispatcher.CurrentDispatcher);
            myClickWaitTimer.Stop();
        }

        private void mainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PageNumber")
            {
                PageNumberBox.Text = mainViewModel.PageNumber;
            }
        }

        private void OpenFileBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = Environment.CurrentDirectory;
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {

                OpenFile(dlg.FileName);
            }
        }

        private void OpenDumpFile(string fName)
        {
            FileNameDump = fName;
            AddFileHistory(fName);
        }

        private void OpenFile(string fName)
        {
            radRibbonBar1.Title = fName;
            string Extension = System.IO.Path.GetExtension(fName);
            if (Extension == ".gml") OpenXMLFile(fName);
            else OpenDumpFile(fName);
        }

        private void AddFileHistory(string fName)
        {
            if (Properties.Settings.Default.LastFiles == null) Properties.Settings.Default.LastFiles = new StringCollection();
            if ((Properties.Settings.Default.LastFiles.Count <= 10) && (Properties.Settings.Default.LastFiles.IndexOf(fName) == -1))
            {
                if (Properties.Settings.Default.LastFiles.Count == 10) Properties.Settings.Default.LastFiles.RemoveAt(9);
                Properties.Settings.Default.LastFiles.Insert(0, fName);
                RebuildAppMenu();
            }
        }

        private void RebuildAppMenu()
        {
            AppMenuPanel.Children.Clear();
            RadGroupHeader FileListHeader = new RadGroupHeader();
            FileListHeader.Content = "Последние файлы";
            AppMenuPanel.Children.Add(FileListHeader);
            if (Properties.Settings.Default.LastFiles == null) Properties.Settings.Default.LastFiles = new StringCollection();
            foreach (string curr in Properties.Settings.Default.LastFiles)
            {
                RadRibbonButton item = new RadRibbonButton();
                item.Content = curr;
                item.Click += (s, e) =>
                {
                    OpenFile(item.Content.ToString());
                };
                AppMenuPanel.Children.Add(item);
            }
        }

        private void BuildTableBtn_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            DateTime dt1 = DateTime.Now;
            if (CreateGraph(FileNameDump))
            {
                SetVertexMark();
                BuildGridView();
                DateTime dt2 = DateTime.Now;
                Status.Text = "Таблица переходов построена за " + (dt2 - dt1) /*DateTime2Str(dt2 - dt1) + " секунд"*/;
                Status.Foreground = new SolidColorBrush(Colors.DarkBlue);
                RadProgress.IsIndeterminate = false;
            }
            Cursor = Cursors.Arrow;
            GraphLayoutBuilded = false;
        }

        private void SetVertexMark()
        {
            foreach (VertexExp ve in g.Vertices)
                ve.IsEndVertex = g.IsOutEdgesEmpty(ve);
        }

        public string DateTime2Str(TimeSpan dt)
        {
            return dt.TotalSeconds.ToString("F2");
        }

        private bool CreateGraph(string fileName)
        {
            if ((fileName == "") || !File.Exists(fileName))
            {
                Status.Text = "Файл не указан или не существует";
                Status.Foreground = new SolidColorBrush(Colors.Red);
                return false;
            }

            //g = new AdjacencyGraph<VertexExp, TaggedEdge<VertexExp, string>>();
            g = new BidirectionalGraph<object, IEdge<object>>();
            int StateNumber = 0;
            int EndStateNumber = 0;
            int LinkNumber = 0;

            TimeSpan StateSpan = TimeSpan.Zero;
            TimeSpan LinkSpan = TimeSpan.Zero;
            Status.Text = "Препроцессинг";

            using (StreamReader sr = new StreamReader(fileName, Encoding.GetEncoding(1251)))
            {
                string line;
                long Size = sr.BaseStream.Length;
                RadProgress.Minimum = 0;
                RadProgress.Maximum = Size;
                InputChars.Clear();
                int type = 1;
                FastGraph.Clear();
                FastState source = null;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line == "") continue;
                    if (line.Contains("State:"))
                    {
                        DateTime dt = DateTime.Now;

                        if (StateNumber == 0) type = 0; 
                        else type = 1;
                        StateNumber++;
                        int ind = line.IndexOf(':');
                        int sourceId = int.Parse(line.Substring(ind + 2, line.Length - ind - 2));

                        if (FastGraph[sourceId] == null)
                        {
                            source = new FastState(sourceId, (FastStateType)type);
                            FastGraph.Add(sourceId, source);
                            source.Vertex = new VertexExp(source.Id, (int)source.Type);
                            g.AddVertex(source.Vertex);

                        }
                        else
                        {
                            source = FastGraph[sourceId] as FastState;
                            source.Type = (FastStateType)type;
                        }

                        if (StateNumber % 1000 == 0)
                        {
                            RadProgress.Value = sr.BaseStream.Position;
                            DoEvents();
                        }

                        StateSpan += DateTime.Now - dt;
                        continue;
                    }
                    if (line.Contains("END STATE, Id ="))
                    {
                        EndStateNumber++;
                        //type = -1;
                        source.Type = FastStateType.End;
                        source.Vertex.Type = -1;
                        continue;
                    }
                    if (line.Contains("] ->"))
                    {
                        DateTime dt = DateTime.Now;

                        LinkNumber++;
                        int ind1 = line.IndexOf("[");
                        int ind2 = line.IndexOf("]");
                        string caption = line.Substring(ind1 + 1, ind2 - ind1 - 1);
                        if (caption.Length > 11)
                            caption = "break";
                        int Input = InputChars.IndexOf(caption);
                        if (Input == -1)
                        {
                            InputChars.Add(caption);
                            Input = InputChars.Count - 1;
                        }
                        int destId = int.Parse(line.Substring(line.IndexOf("] -> ") + 5));

                        FastState dest = null;
                        if (FastGraph[destId] == null)
                        {
                            dest = new FastState(destId, FastStateType.Normal);
                            FastGraph.Add(destId, dest);
                            dest.Vertex = new VertexExp(dest.Id, (int)dest.Type);
                            g.AddVertex(dest.Vertex);
                        }
                        else
                        {
                            dest = FastGraph[destId] as FastState;
                        }

                        FastEdge e = null;
                        bool IsExist = false;
                        foreach (FastEdge curr in source.Edges)
                        {
                            if (curr.Target == dest)
                            {
                                curr.AddInput(Input);
                                IsExist = true;
                                e = curr;
                                break;
                            }
                        }
                        if (!IsExist) e = source.AddEdge(dest, Input);

                        foreach (FastEdge curr in dest.Edges)
                        {
                            if (curr.Target == source)
                            {
                                curr.Type = 1;
                                e.Type = 2;
                            }
                        }

                        LinkSpan += DateTime.Now - dt;
                    }
                }
                RadProgress.Value = sr.BaseStream.Position;
                Status.Text = "Обработка";
                DoEvents();
                //MessageBox.Show("FastGraph + AddVertex done for " + (StateSpan + LinkSpan).TotalSeconds.ToString("F3"));
                int i = 0;
                RadProgress.Maximum = FastGraph.Count;
                RadProgress.Value = 0;
                DoEvents();
                DateTime dtx = DateTime.Now;
                foreach (DictionaryEntry de in FastGraph)
                {
                    FastState fs = de.Value as FastState;
                    VertexExp src = fs.Vertex;
                    foreach (FastEdge fe in fs.Edges)
                    {
                        VertexExp dst = (FastGraph[fe.Target.Id] as FastState).Vertex;
                        string tag = "";
                        for(int j=0; j<fe.Input.Count; j++)
                        {
                            if (j>0) tag += ", ";
                            tag += InputChars[fe.Input[j]];
                        }
                        var e = new EdgeExp(src, dst, tag);
                        e.Type = fe.Type;
                        g.AddEdge(e);
                    }
                    i++;
                    if (i % 1000 == 0)
                    {
                        RadProgress.Value = i;
                        DoEvents();
                    }
                }
                RadProgress.Value = i;
                Status.Text = "Построение таблицы переходов";
                RadProgress.IsIndeterminate = true;
                DoEvents();
                TimeSpan dts = TimeSpan.Zero;
                dts += DateTime.Now - dtx;
                //MessageBox.Show("AddEdge done for " + dts.TotalSeconds.ToString("F3"));

            }
/*
            using (var reader = new StreamReader(fileName, Encoding.GetEncoding(1251)))
            {
                string file = reader.ReadToEnd();
                var mc = Regex.Matches(file, @"State:.*?(?=((State:)|($)))",
                                   RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (mc.Count == 0)
                {
                    Status.Text = "Неверный формат файла";
                    Status.Foreground = new SolidColorBrush(Colors.Red);
                    return false;
                }

                RadProgress.Minimum = 0;
                RadProgress.Maximum = mc.Count;
                InputChars.Clear();

                for (var i = 0; i < mc.Count; i++)
                {
                    int type = 1;
                    if (Regex.IsMatch(mc[i].Value, "END STATE", RegexOptions.IgnoreCase)) type = -1;
                    if (i == 0) type = 0;

                    var state = Regex.Match(mc[i].Value, @"state: \d+", RegexOptions.IgnoreCase);
                    if (state.Success)
                    {
                        int ind = state.Value.IndexOf(':');
                        int sourceId = int.Parse(state.Value.Substring(ind + 2, state.Value.Length - ind - 2));
                        VertexExp source = VertexExp.AddVertex(sourceId, type, true, g);
                        var mcDest = Regex.Matches(mc[i].Value, @"\[(.*?)\] -> (\d+)",
                                                  RegexOptions.Singleline | RegexOptions.IgnoreCase);

                        for (int j = 0; j < mcDest.Count; j++)
                        {
                            string caption = mcDest[j].Groups[1].Value;
                            if (caption.Length > 11)
                                caption = "break";
                            if (InputChars.IndexOf(caption) == -1)
                            {
                                InputChars.Add(caption);
                            }
                            int destId = int.Parse(mcDest[j].Groups[2].Value);
                            var dest = VertexExp.AddVertex(destId, 1, false, g);
                            //var e = new TaggedEdge<VertexExp, string>(source, dest, caption);
                            var e = new EdgeExp(source, dest, caption);

                            foreach (EdgeExp curr in g.OutEdges(dest))
                            {
                                if (curr.Target == source)
                                {
                                    curr.Type = 1;
                                    e.Type = 2;
                                }
                            }

                            bool IsExist = false;
                            foreach (EdgeExp curr in g.OutEdges(source))
                            {
                                if (curr.Target == dest)
                                {
                                    curr.Tag += ", " + caption;
                                    IsExist = true;
                                    break;
                                }
                            }
                            if (!IsExist) g.AddEdge(e);

                        }
                    }
                    if ((i + 1) % 50 == 0)
                    {
                        RadProgress.Value = i + 1;
                        DoEvents();
                    }
                }
                RadProgress.Value = mc.Count;
                DoEvents();
             }
 */ 
            return true;
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }

        private void BuildGridView()
        {
            dg.Columns.Clear();
            DataGridTextColumn dgc0 = new DataGridTextColumn();
            dgc0.Binding = new Binding("Name");
            dgc0.Header = "Состояние";
            dgc0.Width = 70;
            dg.Columns.Add(dgc0);
            currentSortColumn = dgc0;
            for (int i = 0; i < InputChars.Count; i++)
            {
                DataGridTextColumn dgc = new DataGridTextColumn();
                //dgc.Binding = new Binding("NextStateDescr(" + i.ToString() + ")");
                dgc.Binding = new Binding("Edges")
                {
                    Converter = new NextStateDescrConverter(),
                    ConverterParameter = i
                };
                    
                // Background="{Binding Path=VertexColor, Mode=TwoWay, Converter={StaticResource cc}}"
                dgc.Header = InputChars[i];
                dgc.Width = 40;
                dg.Columns.Add(dgc);
            }
            mainViewModel.BuildStates(FastGraph, InputChars.Count);
        }

        private void SaveFileBtn_Click(object sender, RoutedEventArgs e)
        {
            if (g == null)
            {
                Status.Text = "Сначала должна быть построена таблица переходов";
                Status.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "GraphML|*.gml";
            dlg.InitialDirectory = Environment.CurrentDirectory;
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                switch (System.IO.Path.GetExtension(dlg.FileName))
                {
                    case ".gml":
                        using (XmlWriter xwriter = XmlWriter.Create(dlg.FileName))
                        {
                            GMLExpSerializer serializer = new GMLExpSerializer(xwriter, g);
                            serializer.Serialize();
                        }
                        break;
                        /*
                    case ".gml":
                        using (XmlWriter xwriter = XmlWriter.Create(dlg.FileName))
                            g.SerializeToGraphML<VertexExp, TaggedEdge<VertexExp, string>, AdjacencyGraph<VertexExp, TaggedEdge<VertexExp, string>>>(xwriter);
                        break;
                    case ".xml":
                        using (var xwriter = XmlWriter.Create(dlg.FileName))
                            g.SerializeToXml(
                                xwriter,
                                v => v.Id.ToString(), // let's use ID as the vertex ID
                                AlgorithmExtensions.GetEdgeIdentity(g), // let QuickGraph give an id to edges
                                "mygraph", "myvertex", "myedge", "" // names of the graph, vertex, node xml tags and the namespace uri
                                );
                        break;
                         */
                }
                Status.Text = "Файл " + System.IO.Path.GetFileName(dlg.FileName) + " сохранен";
                Status.Foreground = new SolidColorBrush(Colors.DarkBlue);
            }
        }

        private void RadMenuItem_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            mainViewModel.itemCount = int.Parse((sender as RadMenuItem).Header.ToString());
            if (g != null)
            {
                Cursor = Cursors.Wait;
                BuildGridView();
                Cursor = Cursors.Arrow;
            }
        }

        /// <summary>
        /// Initializes the current sort column and direction.
        /// </summary>
        /// <param name="sender">The products data grid.</param>
        /// <param name="e">Ignored.</param>
        private void StatesDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            //DataGrid dataGrid = (DataGrid)sender;

            // The current sorted column must be specified in XAML.
            //currentSortColumn = dataGrid.Columns.Where(c => c.SortDirection.HasValue).Single();
            //currentSortDirection = currentSortColumn.SortDirection.Value;
        }

        /// <summary>
        /// Sets the sort direction for the current sorted column since the sort direction
        /// is lost when the DataGrid's ItemsSource property is updated.
        /// </summary>
        /// <param name="sender">The parts data grid.</param>
        /// <param name="e">Ignored.</param>
        private void StatesDataGrid_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (currentSortColumn != null)
            {
                currentSortColumn.SortDirection = currentSortDirection;
            }
            FastState Curr = mainViewModel.States.FirstOrDefault(state => state.Id == SelVertInGrid);
            if (Curr != null)
            {
                int Ind = mainViewModel.States.IndexOf(Curr);
                dg.SelectedIndex = Ind;
            }
        }

        /// <summary>
        /// Custom sort the datagrid since the actual records are stored in the
        /// server, not in the items collection of the datagrid.
        /// </summary>
        /// <param name="sender">The parts data grid.</param>
        /// <param name="e">Contains the column to be sorted.</param>
        private void StatesDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            MainViewModel mainViewModel = (MainViewModel)DataContext;

            string sortField = String.Empty;

            // Use a switch statement to check the SortMemberPath
            // and set the sort column to the actual column name. In this case,
            // the SortMemberPath and column names match.

            sortField = e.Column.SortMemberPath;

            ListSortDirection direction = (e.Column.SortDirection != ListSortDirection.Ascending) ?
                ListSortDirection.Ascending : ListSortDirection.Descending;

            bool sortAscending = direction == ListSortDirection.Ascending;

            mainViewModel.Sort(sortField, sortAscending);

            if (currentSortColumn != null)
            {
                currentSortColumn.SortDirection = null;
            }

            e.Column.SortDirection = direction;

            currentSortColumn = e.Column;
            currentSortDirection = direction;
        }

        private void GotoPageBtn_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.PageNumber = PageNumberBox.Text;
        }

        private void GraphBtn_Click(object sender, RoutedEventArgs e)
        {
            if (g == null)
            {
                StatusGraph.Text = "Сначала постройте таблицу переходов или откройте файл GML";
                StatusGraph.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            else 
            {
                StatusGraph.Text = "";
                StatusGraph.Foreground = new SolidColorBrush(Colors.Black);
                //Text="{Binding ElementName=graphLayout,Path=LayoutState.ComputationTime,Mode=OneWay,StringFormat='Затраченное время: {0}'}"
                Binding b = new Binding();
                b.ElementName = "graphLayout";
                b.Path = new PropertyPath("LayoutState.ComputationTime");
                b.Mode = BindingMode.OneWay;
                b.StringFormat = "Затраченное время: {0}";
                StatusGraph.SetBinding(TextBlock.TextProperty, b);
            }

            graphLayout.LayoutAlgorithmType = LayoutAlgorithmType;
            graphLayout.EdgeRoutingConstraint = EdgeRoutingConstraint;
            graphLayout.OverlapRemovalConstraint = OverlapRemovalConstraint;
            graphLayout.OverlapRemovalAlgorithmType = OverlapRemovalAlgorithmType;

            /*
            if (!GraphLayoutBuilded)
            {
                graphLayout.Graph = g;
                GraphLayoutBuilded = true;
            }
            else graphLayout.Relayout();
            */

            if (!GraphLayoutBuilded)
            {
                bool isBuilded = true;

                if (tbPath.Text == "")
                {
                    ResetVertexSign();
                    if (g.VertexCount >= 10000)
                    {
                        if (MessageBox.Show("В полученном графе более 10000 вершин. Выполнить построение?", "Информационное сообщение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            graphLayout.Graph = g;
                        else
                            isBuilded = false;
                    }
                    else
                        graphLayout.Graph = g;
                }
                else
                {
                    GetGraphPart(tbPath.Text);
                    if (part != null)
                    {
                        if (part.VertexCount == 0)
                        {
                            MessageBox.Show("Указанный путь не найден.");
                        }
                        else if (part.VertexCount >= 10000)
                        {

                            if (MessageBox.Show("В полученном графе более 10000 вершин. Выполнить построение?", "Информационное сообщение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                graphLayout.Graph = part;
                            else
                                isBuilded = false;
                        }
                        else
                        {
                            graphLayout.Graph = part;
                        }
                    }
                }

                GraphLayoutBuilded = isBuilded;
            }
            else
                graphLayout.Relayout();
            
            EdgesMarksCheck();

            tbVerticesCount.Text = g.VertexCount.ToString();
            tbEdgesCount.Text = g.EdgeCount.ToString();
        }

        #region graph part

        // Сброс признака Sign класса VertexExp
        private void ResetVertexSign()
        {
            if (part == null) return;
            foreach (VertexExp ve in part.Vertices)
                ve.Sign = 0;
        }

        private void GetGraphPart(string path)
        {
            // Сбрасываем признак Sign класса VertexExp
            ResetVertexSign();

            // Находим ссылку на начальную вершину
            VertexExp root = null;
            foreach (VertexExp ve in g.Vertices)
            {
                if (ve.Id == 0)
                {
                    root = ve;
                    break;
                }
            }

            // Разбираем входную строку
            string[] words = path.Split(',');
            //string[] marks = path.Split(',');

            // Выполняем поиск части графа по указанным отметкам
            part = new BidirectionalGraph<object, IEdge<object>>();

            foreach (string word in words)
            {
                //string[] marks = word.Split(',');
                bool isFound = false;
                //CheckVertex(root, marks, -1, ref isFound);
                CheckVertex(root, word, -1, ref isFound);
                SetVertexMinusSign(part, false);
            }

            if (part.VertexCount == 0) return;

            // Добавляем все соседние вершины для начальной
            foreach (EdgeExp e in g.OutEdges(root))
            {
                AddEdgeToPart(part, e);
                SetVertexPlusSign(part, (VertexExp)e.Target);
            }
        }

        // Поиск части графа по указанным отметкам
        private void CheckVertex(VertexExp v, string marks, int i, ref bool isFound)
        {
            i++;

            if (i == marks.Length)
            {
                isFound = true;
                return;
            }

            foreach (EdgeExp e in g.OutEdges(v))
            {
                if ((e.Tag != "break" && e.Tag.Contains(marks[i])) || (e.Tag == "break" && marks[i] == ' '))
                {
                    CheckVertex((VertexExp)e.Target, marks, i, ref isFound);
                    if (isFound)
                    {
                        //if (i == marks.Length - 1 && (!g.IsOutEdgesEmpty(e.Target)))
                        //    ((VertexExp)e.Target).IsEndVertex = true;

                        AddEdgeToPart(part, e);

                        //if (i == marks.Length - 1)
                        SetVertexPlusSign(part, (VertexExp)e.Target);
                    }
                }
            }
        }

        // Установка признака EndVertex класса VertexExp
        private void SetVertexPlusSign(BidirectionalGraph<object, IEdge<object>> temp, VertexExp ve)
        {
            if (g.IsOutEdgesEmpty(ve)) return;
            foreach (EdgeExp e in g.OutEdges(ve))
            {
                if (!temp.ContainsEdge(e))
                {
                    ve.Sign = 1;
                    return;
                }
            }
        }

        private void SetVertexPlusSign(BidirectionalGraph<object, IEdge<object>> temp)
        {
            foreach (VertexExp ve in temp.Vertices)
            {
                SetVertexPlusSign(temp, ve);
            }
        }

        private void SetVertexMinusSign(BidirectionalGraph<object, IEdge<object>> temp, bool withReplace)
        {
            foreach (VertexExp ve in temp.Vertices)
                if (!temp.IsOutEdgesEmpty(ve) && (ve.Sign != 1 || withReplace))
                    ve.Sign = 2;
        }

        // Добавление дуги к графу
        private void AddEdgeToPart(BidirectionalGraph<object, IEdge<object>> temp, EdgeExp e)
        {
            int sourceId = ((VertexExp)e.Source).Id;
            object source = temp.Vertices.SingleOrDefault(ve => (ve as VertexExp).Id == sourceId);
            if (source == null)
                temp.AddVertex(e.Source);

            int targetId = ((VertexExp)e.Target).Id;
            object target = temp.Vertices.SingleOrDefault(ve => (ve as VertexExp).Id == targetId);
            if (target == null)
                temp.AddVertex(e.Target);

            if (!temp.ContainsEdge(e))
                temp.AddEdge(e);

            foreach (EdgeExp ed in g.OutEdges(e.Target))
            {
                try
                {
                    if (temp.ContainsVertex(ed.Target) && !temp.ContainsEdge(ed))
                        temp.AddEdge(ed);
                }
                catch { }
            }

            foreach (EdgeExp ed in g.InEdges(e.Target))
            {
                try
                {
                    if (temp.ContainsVertex(ed.Source) && !temp.ContainsEdge(ed))
                        temp.AddEdge(ed);
                }
                catch { }
            }
        }

        private void tbPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            GraphLayoutBuilded = false;
        }

        private void OpenPathBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = Environment.CurrentDirectory;
            //dlg.Filter = "Текстовые файлы|*.txt";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            { 
                using(StreamReader reader = new StreamReader(new FileStream(dlg.FileName, FileMode.Open)))
                {
                    string mark;
                    StringBuilder sb = new StringBuilder();

                    while ((mark = reader.ReadLine()) != null)
                    { 
                        sb.Append(mark).Append(',');
                    }
                    sb.Remove(sb.Length - 1, 1);
                    tbPath.Text = sb.ToString();
                }
            }
        }

        VertexExp vertex = null;

        private void VertexPlusClick(object sender, MouseButtonEventArgs e)
        {
            //e.Handled = true;

            VertexControl vc = (VertexControl)sender;
            vertex = (VertexExp)vc.Vertex;

            if (vertex.Sign == 0)
                return;

            Point mousePos = e.GetPosition(vc);
            double width = vc.ActualWidth;
            if (mousePos.X > width - 17 && mousePos.X < width - 7 && mousePos.Y < 15 && mousePos.Y > 5)
            {
                IsSingleClick = true;
                if (e.ClickCount > 1)
                {
                    IsSingleClick = false;
                    DoubleClickHandler();
                }
                else
                {
                    myClickWaitTimer.Start();
                }
            }
        }

        private void SingleClickHandler()
        {
            if (vertex == null) return;

            BidirectionalGraph<object, IEdge<object>> temp = new BidirectionalGraph<object, IEdge<object>>();
            temp.AddVerticesAndEdgeRange(part.Edges);

            if (vertex.Sign == 1)
            {
                foreach (EdgeExp e in g.OutEdges(vertex))
                {
                    AddEdgeToPart(temp, e);
                    SetVertexPlusSign(temp, (VertexExp)e.Target);
                }
                vertex.Sign = 2;
            }
            else if (vertex.Sign == 2)
            {
                HideVertex(temp, vertex, vertex);
                //vertex.Sign = 1;
                SetVertexPlusSign(temp);
            }

            graphLayout.Graph = temp;
            EdgesMarksCheck();
            part = temp;
        }

        private void DoubleClickHandler()
        {
            if (vertex == null) return;

            BidirectionalGraph<object, IEdge<object>> temp = new BidirectionalGraph<object, IEdge<object>>();
            temp.AddVerticesAndEdgeRange(part.Edges);

            ShowVertex(temp, vertex);
            SetVertexMinusSign(temp, true);
            //vertex.Sign = 2;
            graphLayout.Graph = temp;
            EdgesMarksCheck();
            part = temp;
        }

        private void ShowVertex(BidirectionalGraph<object, IEdge<object>> temp, VertexExp ve)
        {
            foreach (EdgeExp e in g.OutEdges(ve))
            {
                //if (((VertexExp)e.Source).Id != ((VertexExp)e.Target).Id)
                //    ShowVertex(temp, (VertexExp)e.Target);

                if (!temp.ContainsVertex(e.Target) || !temp.ContainsEdge(e))
                {
                    AddEdgeToPart(temp, e);
                    //SetVertexSign(temp, (VertexExp)e.Target, 2);
                    ShowVertex(temp, (VertexExp)e.Target);
                }
            }
        }

        private void HideVertex(BidirectionalGraph<object, IEdge<object>> temp, VertexExp root, VertexExp ve)
        {
            foreach (EdgeExp e in g.OutEdges(ve))
            {
                bool isVertexPresent = false;
                bool isEdgePresent = false;
                bool process = true;
                if (ve == root)
                {
                    try
                    {
                        foreach (EdgeExp ed in temp.OutEdges(e.Target))
                        {
                            if (ed.Target == root)
                            {
                                temp.RemoveEdge(e);
                                process = false;
                            }
                        }
                    }
                    catch { }
                }

                if (process)
                {
                    try
                    {
                        if (e.Target != root)
                            isVertexPresent = temp.ContainsVertex(e.Target);
                    }
                    catch { }

                    try
                    {
                        isEdgePresent = temp.ContainsEdge(e);
                    }
                    catch { }

                    if (isVertexPresent || isEdgePresent)
                    {
                        if (isEdgePresent) temp.RemoveEdge(e);
                        if (isVertexPresent) temp.RemoveVertex(e.Target);
                        HideVertex(temp, root, (VertexExp)e.Target);
                    }
                }
            }
        }

        private void mouseWaitTimer_Tick(object sender, EventArgs e)
        {
            myClickWaitTimer.Stop();
            if (IsSingleClick) SingleClickHandler();
        }

        #endregion

        private void radRibbonBar1_SelectedTabChanged(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            if (radRibbonBar1.SelectedTab.Name == "tbGraph")
            {
                dpGraph.Visibility = System.Windows.Visibility.Visible;
                svState.Visibility = System.Windows.Visibility.Collapsed;
                sbGraph.Visibility = System.Windows.Visibility.Visible;
                sbState.Visibility = System.Windows.Visibility.Collapsed;
                if (FileNameXML != "") radRibbonBar1.Title = FileNameXML;
            }
            else
            {
                dpGraph.Visibility = System.Windows.Visibility.Collapsed;
                svState.Visibility = System.Windows.Visibility.Visible;
                sbGraph.Visibility = System.Windows.Visibility.Collapsed;
                sbState.Visibility = System.Windows.Visibility.Visible;
                if (FileNameDump != "") radRibbonBar1.Title = FileNameDump;
            }
        }

        private void EndStatesHighlightCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            EndStatesColorChange();
        }

        private void EndStatesColorPicker_SelectedColorChanged(object sender, EventArgs e)
        {
            EndStatesColorChange();
        }

        private void EndStatesColorChange()
        {
            Color color;
            if (EndStatesHighlightCheckBox.IsChecked == true)
                color = EndStatesColorPicker.SelectedColor;
            else
                color = Colors.Transparent;

            foreach (VertexExp v in g.Vertices)
            {
                if (v.Type == -1)
                {
                    v.VertexColor = color;
                }
            }
        }

        private void root_Loaded(object sender, RoutedEventArgs e)
        {
//            LayoutAlgorithmBox.SelectedIndex = 1;
            RebuildAppMenu();
        }

        private void OpenGraphBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
//            dlg.Filter = "GraphML|*.gml|Custom XML|*.xml";
            dlg.Filter = "GraphML|*.gml";
            dlg.InitialDirectory = Environment.CurrentDirectory;
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                OpenFile(dlg.FileName);
            }
        }

        private void OpenXMLFile(string fName)
        {
            GraphLayoutBuilded = false;
            FileNameXML = fName;
            AddFileHistory(fName);
            using (XmlReader reader = XmlReader.Create(fName))
            {
                FastGraph.Clear();
                g = new BidirectionalGraph<object, IEdge<object>>();
                GMLExpDeserializer deserializer = new GMLExpDeserializer(reader);
                g = deserializer.Deserialize();
            }
        }

        private void RelayoutBtn_Click(object sender, RoutedEventArgs e)
        {
            graphLayout.Relayout();
        }

        private void ContinueLayoutBtn_Click(object sender, RoutedEventArgs e)
        {
            graphLayout.ContinueLayout();
        }

        private void ApplicationMenu_Loaded(object sender, RoutedEventArgs e)
        {
            if ((Properties.Settings.Default.LastFiles == null) || (Properties.Settings.Default.LastFiles.Count == 0))
                AppMenu.Height = 40;
            else AppMenu.Height = 270;
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void EdgesMarksCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            EdgesMarksCheck();
        }

        private void EdgesMarksCheck()
        {
            foreach (var el in graphLayout.Children)
            {
                if (el is EdgeControl)
                {
                    (el as EdgeControl).ToolTip = ((el as EdgeControl).Edge as EdgeExp).Tag;
                    (el as EdgeControl).Tag = "";
                    if (EdgesMarksCheckBox.IsChecked == true)
                        (el as EdgeControl).Tag = ((el as EdgeControl).Edge as EdgeExp).Tag;

                    (el as EdgeControl).Type = ((el as EdgeControl).Edge as EdgeExp).Type;
                }
            }
        }

        private void root_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void DijkstraPathCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (DijkstraPathCheckBox.IsChecked == true)
                graphLayout.HighlightAlgorithmType = "Dijkstra";
            else
                graphLayout.HighlightAlgorithmType = "Simple";
        }

        private void FullScreen()
        {
            IsFullScreen = true;
            this.WindowState = WindowState.Normal;
            this.WindowStyle = WindowStyle.None;
            this.WindowState = WindowState.Maximized;
            radRibbonBar1.Visibility = Visibility.Collapsed;
            sbState.Visibility = Visibility.Collapsed;
            sbGraph.Visibility = Visibility.Collapsed;
            this.Topmost = true;
        }

        private void NormScreen()
        {
            IsFullScreen = false;
            this.WindowState = WindowState.Normal;
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.Topmost = false;
            this.WindowState = WindowState.Maximized;
            radRibbonBar1.Visibility = Visibility.Visible;
            sbState.Visibility = Visibility.Visible;
            sbGraph.Visibility = Visibility.Visible;
        }

        private void root_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.F11) return;
            if (!IsFullScreen) FullScreen();
            else NormScreen();
        }

        private void dg_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(e.OriginalSource is TextBlock)) return;
            string CellContent = (e.OriginalSource as TextBlock).Text;
            CellContent = CellContent.Replace(" ", "");
            CellContent = CellContent.Replace("*", "");
            CellContent = CellContent.Replace(">", "");
            try
            {
                SelVertInGrid = int.Parse(CellContent);
            }
            catch
            {
                return;
            }
            mainViewModel.PageNumber = (SelVertInGrid / mainViewModel.itemCount + 1).ToString();
        }

        private void FullScreenLink_Click(object sender, RoutedEventArgs e)
        {
            if (!IsFullScreen)
            {
                FullScreen();
                FullScreenLink.Inlines.Clear();
                FullScreenLink.Inlines.Add(new Run("Нормальный вид"));
            }
            else
            {
                NormScreen();
                FullScreenLink.Inlines.Clear();
                FullScreenLink.Inlines.Add(new Run("Во весь экран"));
            }
        }


        private void SelectAlgorithmBtn_Click(object sender, RoutedEventArgs e)
        {
            SelectAlgorithmWindow saw = new SelectAlgorithmWindow(graphLayout, this);
            saw.ShowDialog();
        }

        private void AlgorithmParametersBtn_Click(object sender, RoutedEventArgs e)
        {
            AlgorithmParametersWindow apw = new AlgorithmParametersWindow(graphLayout, LayoutAlgorithmType);
            apw.ShowDialog();
        }

    }
}
