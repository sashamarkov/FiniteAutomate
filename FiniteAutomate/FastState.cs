using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Data;
using System.Globalization;

namespace FiniteAutomate
{
    //класс для одного состояния конечного автомата
    public class FastState
    {
        public int Id;
        public FastStateType Type;
        public List<FastEdge> Edges { get; set;}
        public VertexExp Vertex;
        //public string[] NextStateDescr { get; set; }

        public FastState(int Id, FastStateType Type)
        {
            this.Id = Id;
            this.Type = Type;
            Edges = new List<FastEdge>();
        }

        //добавление ребра для случая, когда состояние является фокусным
        public FastEdge AddEdge(FastState Target, int Input)
        {
            FastEdge fe = new FastEdge(Target, Input);
            Edges.Add(fe);
            return fe;
        }

        //добавление меток в номера состояний, которые являются конечными
        //и в номер состояния, которое является начальным
        public string Name
        {
            get
            {
                string name = " " + Id.ToString() + " ";
                switch (Type)
                {
                    case FastStateType.Start: return ">" + name;
                    case FastStateType.End: return name + "*";
                }
                return name;
            }
        }

        //получение номеров смежных состояний
        public string NextStateDescr(int i)
        {
                foreach (FastEdge fe in Edges)
                {
                    if (fe.Input.IndexOf(i) != -1)
                    {
                        return fe.Target.Name;
                    }
                }
                return "";
        }

        //получение внутренних идентификаторов смежных состояний
        public int NextState(int i)
        {
            foreach (FastEdge fe in Edges)
            {
                if (fe.Input.IndexOf(i) != -1)
                {
                    return fe.Target.Id;
                }
            }
            return -1;
        }
    }

    public class NextStateDescrConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is List<FastEdge>)
            {
                if (parameter is int)
                {
                    int i = (int)parameter;
                    List<FastEdge> list = (List<FastEdge>)value;
                    foreach (FastEdge fe in list)
                    {
                        if (fe.Input.IndexOf(i) != -1)
                        {
                            return fe.Target.Name;
                        }
                    }
                    return "";
                }
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    //класс для хранения переходов
    public class FastEdge
    {
        public FastState Target;
        public List<int> Input = new List<int>();
        public int Type;

        public FastEdge(FastState Target, int Input)
        {
            this.Target = Target;
            AddInput(Input);
        }

        public void AddInput(int Input)
        {
            this.Input.Add(Input);
        }
    }

    //пересисление для хранения типа состояния
    public enum FastStateType 
    {
        End = -1,
        Start = 0,
        Normal = 1
    }
}
