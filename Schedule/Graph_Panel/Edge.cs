using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphs.GraphPanel
{
    public class Edge
    {
        private static Color DEFAULT_COLOR_EDGE = Color.Black;
        public Color color;
        public int tickness { get; set; }
        public bool TwoSided { get; set; }
        public Vertex v_from { get; set; }
        public Vertex v_to { get; set; }

        public Edge(Vertex v_from, Vertex v_to, bool twoSide = false ,Color? color_edge = null)
        {
            if(color_edge == null)
                color = DEFAULT_COLOR_EDGE;
            else
                color = (Color)color_edge;

            TwoSided = twoSide;

            this.v_from = v_from;
            this.v_to = v_to;
        }

        public void ChangeV_From(Vertex v)
        {
            this.v_from = v;
        }
        public void ChangeV_To(Vertex v)
        {
            this.v_to = v;
        }
    }
}
