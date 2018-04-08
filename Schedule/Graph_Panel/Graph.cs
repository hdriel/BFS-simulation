using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Graphs.GraphPanel
{
    public class Graph
    {
        public Panel main_panel;
        public List<Vertex> V = new List<Vertex>();
        public List<Edge> E = new List<Edge>();
        public bool oneWay { get; set; }
        public int SizeV { get; set; }
        public Graph(ref Panel ContainerPanel, List<string> listV, bool[,] matEdge, bool oneWay, int SizeV = -1, bool isTree = false)
        {
            if (matEdge == null) matEdge = new bool[0, 0];
            main_panel = ContainerPanel;
            V = new List<Vertex>();
            E = new List<Edge>();

            this.SizeV = SizeV;
            this.oneWay = oneWay;
            initVertexes(ref V, listV, main_panel);
            initEdges(ref V, ref E, matEdge);

            setDragOptionToVertexes(ref V, ref main_panel);
            if (isTree)
                relocationTreeV();
            else
                relocationV();
            main_panel.Refresh();
        }

        public void ChangeSize(int s)
        {
            foreach (Vertex v in V)
            {
                v.Size = s;
                v.lable.Height = s;
                v.lable.Width = s;
            }
        }
        public void ClearPanel()
        {
            foreach (Vertex v in V)
            {
                main_panel.Controls.Remove(v.v_panel);
            }
            V = new List<Vertex>();
            E = new List<Edge>();
        }
        public void RemoveFocus()
        {
            foreach (Vertex v in V)
            {
                v.Focus = false;
            }
        }

        private void initVertexes(ref List<Vertex> V, List<string> listV, Panel p)
        {
            Random rnd = new Random();
            if (listV == null) return;
            for (int i = 0; i < listV.Count; i++)
            {
                Vertex v = new Vertex((listV[i]).ToString(), SizeV);
                int sizeV = v.Size;
                int x = 0, y = 0;
                v.Location = new Point(x, y);
                V.Add(v);
            }
        }
        private void initEdges(ref List<Vertex> V, ref List<Edge> E, bool[,] mat)
        {
            if (V == null) V = new List<Vertex>();
            if (E == null) E = new List<Edge>();

            int n = mat.GetLength(0);
            for (int i = 1; i < n; i++)
            {
                for (int j = 1; j < n; j++)
                {
                    if (mat[i, j])
                    {
                        Edge e1 = V[i - 1].AddNeighbor_son(V[j - 1], oneWay);
                        e1.TwoSided = !oneWay;
                        E.Add(e1);
                    }
                }
            }
        }

        private void initVertexesMock(ref List<Vertex> V, int n, Panel p)
        {
            Random rnd = new Random();
            for (int i = 0; i < n; i++)
            {
                Vertex v = new Vertex((i + 1).ToString());
                if (i == 0) v.Focus = true;
                int sizeV = v.Size;
                int x = rnd.Next(0 + sizeV, p.Width - sizeV);   // month: >= 100 and < 1000
                int y = rnd.Next(0 + sizeV, p.Height - sizeV);  // month: >= 100 and < 600
                v.Location = new Point(x, y);
                V.Add(v);
            }
        }
        private void initEdgesMock(ref List<Vertex> V, ref List<Edge> E)
        {
            Random rnd = new Random();
            if (V == null) V = new List<Vertex>();
            int n = rnd.Next(0, V.Count * 2);
            for (int i = 0; i < n; i++)
            {
                int j1 = rnd.Next(0, V.Count);
                int j2 = rnd.Next(0, V.Count);
                bool t = rnd.Next(0, 1) == 0 ? true : false;

                Edge e = V[j1].AddNeighbor_son(V[j2], oneWay);
                e.TwoSided = !oneWay;
                E.Add(e);
            }
        }

        /* method to paint the vertexes and edges on the panel */
        private void panel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = main_panel.CreateGraphics();
            g.Clear(main_panel.BackColor);

            Pen penArrow;
            Pen penNotDirections;
            List<Pen> penCircleAround = new List<Pen>();
            int thick0 = 2;
            int thick1 = 3;
            int thickPen = 5;
            penCircleAround.Add(new Pen(Color.Black, thick0));
            penCircleAround.Add(new Pen(Color.Black, thick1));

            foreach (Vertex v in V)
            {
                penCircleAround[1] = new Pen(v.around, thick1);

                if (!isFreePlace(v, v.v_panel.Location.X, v.v_panel.Location.Y))
                {
                    penCircleAround[0].Color = Color.Red;
                    penCircleAround[1].Color = Color.Red;
                }
                g.DrawEllipse(penCircleAround[0], v.v_panel.Location.X - thick0, v.v_panel.Location.Y - thick0, v.Size + thick0, v.Size + thick0);

                if (v.Focus)
                {
                    int space = thick0 + thick1;
                    g.DrawEllipse(penCircleAround[1], v.v_panel.Location.X - space, v.v_panel.Location.Y - space, v.Size + space + thick1, v.Size + space + thick1);
                }
            }

            foreach (Edge ed in E)
            {
                penArrow = new Pen(ed.color, thickPen);
                //penArrow.StartCap = LineCap.ArrowAnchor;
                GraphicsPath capPath = new GraphicsPath();
                int lenArrow = 2;
                Point[] arrow_tringle = { new Point(-lenArrow, 0), new Point(lenArrow, 0), new Point(0, lenArrow) };
                capPath.AddLine(arrow_tringle[0], arrow_tringle[1]);   
                capPath.AddLine(arrow_tringle[1], arrow_tringle[2]);  
                capPath.AddLine(arrow_tringle[2], arrow_tringle[0]); 
                penArrow.CustomStartCap = new System.Drawing.Drawing2D.CustomLineCap(null, capPath);

                penArrow.EndCap = LineCap.RoundAnchor;

                penNotDirections = new Pen(ed.color, thickPen);

                Point p0 = new Point(ed.v_from.v_panel.Location.X + ed.v_from.v_panel.Width / 2,
                                     ed.v_from.v_panel.Location.Y + ed.v_from.v_panel.Height / 2);

                Point p1 = new Point(ed.v_to.v_panel.Location.X + ed.v_to.v_panel.Width / 2,
                                     ed.v_to.v_panel.Location.Y + ed.v_to.v_panel.Height / 2);

                if (ed.v_from == ed.v_to)
                {
                    int radius = ed.v_from.v_panel.Width;
                    //                                    x                        y                           weith  hieght end start 
                    g.DrawArc(penArrow, ed.v_from.Location.X - radius / 2, ed.v_from.Location.Y - radius / 2, radius - 20 , radius, 100, 250); // 95 , 230 - without capPath
                }
                else
                {
                    if (ed.TwoSided)
                    {
                        g.DrawLine(penNotDirections, p1, p0);
                    }
                    else
                    {
                        bool hasreverse = false;
                        foreach (Edge edrev in E)
                        {
                            if (ed.v_from == edrev.v_to && ed.v_to == edrev.v_from)
                            {
                                hasreverse = true;
                                break;
                            }
                        }


                        try
                        {
                            int x1, y1, x2, y2, cx1, cx2, cy1, cy2;
                            x1 = ed.v_from.v_panel.Location.X;
                            cx1 = x1 + ed.v_from.v_panel.Width / 2;
                            y1 = ed.v_from.v_panel.Location.Y;
                            cy1 = y1 + ed.v_from.v_panel.Height / 2;
                            x2 = ed.v_to.v_panel.Location.X;
                            cx2 = x2 + ed.v_to.v_panel.Width / 2;
                            y2 = ed.v_to.v_panel.Location.Y;
                            cy2 = y2 + ed.v_to.v_panel.Height / 2;
                            {
                                int spaceStartArrowFromVertex = 15; // 5
                                int spaceToSideLine = 100;
                                int d = (ed.v_from.v_panel.Height / 2 + spaceStartArrowFromVertex);

                                if (cx1 <= cx2 && cy1 >= cy2) // rotate right-top // work!
                                {
                                    Point c_from = new Point(cx1 + d, cy1);
                                    Point c_to = new Point(cx2 - d, cy2);
                                    int x = cx2 > cx1 ? cx2 - cx1 : cx1 - cx2;
                                    int y = cy2 > cy1 ? cy2 - cy1 : cy1 - cy2;
                                    float angle = (float)Math.Atan((double)(y) / x);

                                    p0 = rotate_point(cx1, cy1, -angle, c_from);
                                    p1 = rotate_point(cx2, cy2, -angle, c_to);

                                    if (hasreverse)
                                    {
                                        p0 = rotate_point(cx1, cy1, -angle + spaceToSideLine, c_from);
                                        p1 = rotate_point(cx2, cy2, -angle - spaceToSideLine, c_to);
                                    }
                                }
                                else if (cx1 >= cx2 && cy1 >= cy2) // rotate top-left // work!
                                {
                                    Point c_from = new Point(cx1 - d, cy1);
                                    Point c_to = new Point(cx2 + d, cy2);
                                    int x = cx2 > cx1 ? cx1 + cx2 : cx1 - cx2;
                                    int y = cy2 > cy1 ? cy1 + cy2 : cy1 - cy2;
                                    float angle = (float)Math.Atan((double)(y) / x);

                                    p0 = rotate_point(cx1, cy1, angle, c_from);
                                    p1 = rotate_point(cx2, cy2, angle, c_to);

                                    if (hasreverse)
                                    {
                                        p0 = rotate_point(cx1, cy1, angle + spaceToSideLine, c_from);
                                        p1 = rotate_point(cx2, cy2, angle - spaceToSideLine, c_to);
                                    }
                                }
                                else if (cx1 >= cx2 && cy1 <= cy2) // rotate left-bottom // work!
                                {
                                    Point c_from = new Point(cx1, cy1 + d);
                                    Point c_to = new Point(cx2, cy2 - d);
                                    int x = cx2 < cx1 ? cx2 - cx1 : cx1 - cx2;
                                    int y = cy1 > cy2 ? cy2 + cy1 : cy1 - cy2;
                                    float angle = (float)Math.Atan((double)(x) / y);

                                    p0 = rotate_point(cx1, cy1, angle, c_from);
                                    p1 = rotate_point(cx2, cy2, angle, c_to);

                                    if (hasreverse)
                                    {
                                        p0 = rotate_point(cx1, cy1, angle + spaceToSideLine, c_from);
                                        p1 = rotate_point(cx2, cy2, angle - spaceToSideLine, c_to);

                                    }
                                }
                                else if (cx1 <= cx2 && cy1 <= cy2) // rotate bottom-right // work!
                                {
                                    Point c_from = new Point(cx1, cy1 + d);
                                    Point c_to = new Point(cx2, cy2 - d);
                                    int x = cx2 > cx1 ? cx2 - cx1 : cx1 + cx2;
                                    int y = cy1 > cy2 ? cy2 - cy1 : cy1 - cy2;
                                    float angle = (float)Math.Atan((double)(x) / y);

                                    p0 = rotate_point(cx1, cy1, angle, c_from);
                                    p1 = rotate_point(cx2, cy2, angle, c_to);

                                    if (hasreverse)
                                    {
                                        p0 = rotate_point(cx1, cy1, angle + spaceToSideLine, c_from);
                                        p1 = rotate_point(cx2, cy2, angle - spaceToSideLine, c_to);
                                    }
                                }
                            }
                            {
                                g.DrawLine(penArrow, p1, p0);
                            }
                        }
                        catch (Exception)//Exception exp)
                        {
                            //MessageBox.Show("ERROR Asin/Acos: \n\n" + exp.Message);
                        }
                    }
                }
            }

            g.Dispose();
        }
       
        /* method to get point on the circle of the vertex */
        private Point rotate_point(float cx, float cy, float angle, Point p)
        {
            float s = (float)Math.Sin(angle);
            float c = (float)Math.Cos(angle);

            // translate point back to origin:
            p.X -= (int)cx;
            p.Y -= (int)cy;

            // rotate point
            float xnew = p.X * c - p.Y * s;
            float ynew = p.X * s + p.Y * c;

            // translate point back:
            p.X = (int)(xnew + cx);
            p.Y = (int)(ynew + cy);
            return p;
        }
        /* Find the angle of the p0-c-p1 on the Horizontal line */
        public double find_angle(Point p0, Point p1, Point c)
        {
            var p0c = Math.Sqrt(Math.Pow(c.X - p0.X, 2) +
                                Math.Pow(c.Y - p0.Y, 2)); // p0->c (b)   
            var p1c = Math.Sqrt(Math.Pow(c.X - p1.X, 2) +
                                Math.Pow(c.Y - p1.Y, 2)); // p1->c (a)
            var p0p1 = Math.Sqrt(Math.Pow(p1.X - p0.X, 2) +
                                 Math.Pow(p1.Y - p0.Y, 2)); // p0->p1 (c)
            return Math.Cos((p1c * p1c + p0c * p0c - p0p1 * p0p1) / (2 * p1c * p0c));
        }

        public void VetexCotrols(bool add)
        {
            if (add)
            {
                foreach (Vertex v in V)
                {
                    main_panel.Controls.Add(v.v_panel);
                    v.v_panel.Visible = true;
                }
            }
            else
            {
                foreach (Vertex v in V)
                {
                    main_panel.Controls.Remove(v.v_panel);
                    v.v_panel.Visible = false;
                }
            }
            main_panel.Refresh();
        }

        /* Handler the Drop & Down on Vertexes */
        private bool isDragging = false;
        Point move;

        void c_MouseDownDoubleClick(object sender, MouseEventArgs e)
        {
            Control c = sender as Control;
            move = e.Location;
        }
        void c_MouseDown(object sender, MouseEventArgs e)
        {
            Control c = sender as Control;
            isDragging = true;
            move = e.Location;
        }

        void c_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging == true)
            {
                if (e.Button == MouseButtons.Left)
                {
                    Control c = sender as Control;
                    for (int i = 0; i < V.Count(); i++)
                    {
                        if (c.Equals(V[i].v_panel) || c.Equals(V[i].lable) || c.Equals(V[i].drag))
                        {
                            V[i].v_panel.Left += e.X - move.X;
                            V[i].v_panel.Top += e.Y - move.Y;
                        }
                    }
                }
                else if (e.Button == MouseButtons.Right)
                {
                    for (int i = 0; i < V.Count(); i++)
                    {
                        V[i].v_panel.Left += e.X - move.X;
                        V[i].v_panel.Top += e.Y - move.Y;
                    }
                }
            }
        }
        void c_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            insertVtoFrame();
        }
        private void setDragOptionToVertexes(ref List<Vertex> V, ref Panel p)
        {
            if (V == null)
                V = new List<Vertex>();

            p.Paint += new PaintEventHandler(panel_Paint);
            foreach (Vertex v in V)
            {
                v.v_panel.MouseDown += new MouseEventHandler(c_MouseDown);
                v.v_panel.MouseMove += new MouseEventHandler(c_MouseMove);
                v.v_panel.MouseUp += new MouseEventHandler(c_MouseUp);
                v.v_panel.MouseDoubleClick += new MouseEventHandler(c_MouseDownDoubleClick);

                v.lable.MouseDown += new MouseEventHandler(c_MouseDown);
                v.lable.MouseMove += new MouseEventHandler(c_MouseMove);
                v.lable.MouseUp += new MouseEventHandler(c_MouseUp);
                v.lable.MouseDoubleClick += new MouseEventHandler(c_MouseDownDoubleClick);


                v.drag.MouseDown += new MouseEventHandler(c_MouseDown);
                v.drag.MouseMove += new MouseEventHandler(c_MouseMove);
                v.drag.MouseUp += new MouseEventHandler(c_MouseUp);
                v.drag.MouseDoubleClick += new MouseEventHandler(c_MouseDownDoubleClick);


                v.addToControl(p);
                p.Refresh();
            }
        }

        public void relocationV()
        {
            List<Vertex> didnetHandeled = new List<Vertex>(V);
            List<Vertex> handeled = new List<Vertex>();
            Point CenterTop = new Point((main_panel.Width / 2 - V[0].Size / 2), 5);
            int spaceV_Hor = 30;
            int spaceV_Ver = 50;
            if(V.Count > 0) { 
                V[0].Location = CenterTop;
                handeled.Add(V[0]);
            }

            for (int i = 0; i < V.Count; i++)
            {
                int n_adj = V[i].edges_out.Count;
                for (int j = 0, r = 1, l = 0, r2=1, l2=2; j < n_adj; j++)
                {
                    Vertex A = V[i].edges_out[j].v_to;
                    Vertex S = V[i].edges_out[j].v_from;
                    if (handeled.Find(v => v == A) == null && A.Location.X == 0 && A.Location.Y == 0)
                    {
                        A.Location = new Point(S.Location.X, S.Location.Y + S.Size + spaceV_Ver);
                        if (j <= n_adj / 2)
                        { 
                            if(S.Location.X - l * (A.Size + spaceV_Hor) > 0)
                            { 
                                A.Location = new Point(S.Location.X - l++ * (A.Size + spaceV_Hor), S.Location.Y + S.Size + l*spaceV_Ver);
                            }
                            else
                            {
                                A.Location = new Point(S.Location.X - l2++ * (A.Size + spaceV_Hor), S.Location.Y + 2*(S.Size) + l2 * (int)(spaceV_Ver*1.5));
                            }
                        }
                        else
                        {
                            if (S.Location.X + r * (A.Size + spaceV_Hor) <  main_panel.Width - A.Size)
                            {
                                A.Location = new Point(S.Location.X + r++ * (A.Size + spaceV_Hor), S.Location.Y + S.Size + r * spaceV_Ver);
                            }
                            else
                            {
                                A.Location = new Point(S.Location.X + r2++ * (A.Size + spaceV_Hor), S.Location.Y + 2*(S.Size) + r2 * (int)(spaceV_Ver * 1.5));
                            }
                        }
                    }
                }
                handeled.Add(V[i]);
            }
            Random rnd = new Random();
            int R = 1, R2 = 0, L2 = 0, L = 0, J = 0;
            foreach (Vertex v in V)
            {
                if (v.Location.X == 0 && v.Location.Y == 0)
                { 
                    if(v.edges_out.Count > 0)
                    { 
                        int n_adj = v.edges_out[0].v_to.edges_in.Count;
                        Vertex A = v;
                        Vertex S = v.edges_out[0].v_to;
                        if (J <= n_adj / 2)
                        {
                            Point o = new Point(S.Location.X - L * (A.Size + spaceV_Hor), S.Location.Y + S.Size + L * spaceV_Ver);
                            if (S.Location.X - L * (A.Size + spaceV_Hor) > 0 ) //&& isFreePlace(S, o.X, o.Y))
                            {
                                A.Location = new Point(S.Location.X - L++ * (A.Size + spaceV_Hor), S.Location.Y + S.Size + L * spaceV_Ver);
                            }
                            else
                            {
                                A.Location = new Point(S.Location.X - L2++ * (A.Size + spaceV_Hor), S.Location.Y + 2 * (S.Size) + L2 * (int)(spaceV_Ver * 1.5));
                            }
                        }
                        else
                        {
                            Point o = new Point(S.Location.X + R * (A.Size + spaceV_Hor), S.Location.Y + S.Size +R * spaceV_Ver);
                            if (S.Location.X + R * (A.Size + spaceV_Hor) < main_panel.Width - A.Size)
                            {
                                A.Location = new Point(S.Location.X + R++ * (A.Size + spaceV_Hor), S.Location.Y + S.Size + R * spaceV_Ver);
                            }
                            else
                            {
                                A.Location = new Point(S.Location.X + R2++ * (A.Size + spaceV_Hor), S.Location.Y + 2 * (S.Size) + R2 * (int)(spaceV_Ver * 1.5));
                            }
                        }
                        J++;
                    }
                    else
                    {
                        v.Location = new Point(rnd.Next(0 + v.Size, main_panel.Width - v.Size), rnd.Next(0 + v.Size, main_panel.Width - v.Size));
                    }
                }
            }
            insertVtoFrame();
        }
        public void insertVtoFrame()
        {
            bool wasChanges = false;
            foreach (Vertex v in V)
            {
                if(v.Location.X < 0)
                {
                    v.Location = new Point(0 + 2, v.Location.Y);
                    wasChanges = true;
                }
                if(v.Location.X + v.Size > main_panel.Width)
                {
                    v.Location = new Point(main_panel.Width - v.Size-2, v.Location.Y);
                    wasChanges = true;
                }
                if (v.Location.Y < 0)
                {
                    v.Location = new Point(v.Location.X, 0 + 2);
                    wasChanges = true;
                }
                else if(v.Location.Y + v.Size > main_panel.Height)
                {
                    v.Location = new Point(v.Location.X, main_panel.Height - v.Size - 2);
                    wasChanges = true;
                }
            }
            if (wasChanges)
            {
                main_panel.Refresh();
            }
        }
        public void resertOutsideLocationV()
        {
            foreach (Vertex v in V)
            {
                if (v.Location.X < 0  || v.Location.X + v.Size > main_panel.Width || v.Location.Y < 0 || v.Location.Y + v.Size > main_panel.Height )
                {
                    v.Location = new Point();
                }
            }
        }
        private bool isFreePlace(Vertex ver,int x, int y)
        {
            if (V.Count <= 0) return true;
            foreach (Vertex v in V)
            {
                if ((x < 0 || x > main_panel.Width - v.Size || y < 0 || y > main_panel.Height - v.Size) ||
                    v != ver &&
                    v.v_panel.Location.X - v.v_panel.Width  < x && x < v.v_panel.Location.X + v.v_panel.Width &&
                    v.v_panel.Location.Y - v.v_panel.Height < y && y < v.v_panel.Location.Y + v.v_panel.Height)
                {
                    return false;
                }
            }
            return true;
        }

        public void relocationTreeV()
        {
            List<Vertex> didnetHandeled = new List<Vertex>(V);
            List<Vertex> handeled = new List<Vertex>();
            Point CenterTop = new Point((main_panel.Width / 2 - V[0].Size / 2), 5);
            int spaceV_Hor = 30;
            int spaceV_Ver = 50;
            if (V.Count > 0)
            {
                V[0].Location = CenterTop;
                handeled.Add(V[0]);
            }

            for (int i = 0; i < V.Count; i++)
            {
                int n_adj = V[i].edges_out.Count;
                for (int j = 0, r = 1, l = 0; j < n_adj; j++)
                {
                    Vertex A = V[i].edges_out[j].v_to;
                    Vertex S = V[i].edges_out[j].v_from;
                    switch (n_adj)
                    {
                        case 1: spaceV_Hor = 0; break;
                        case 2: spaceV_Hor = 100; break;
                        case 3: spaceV_Hor = 80; break;
                        case 4: spaceV_Hor = 50; break;
                        case 5:
                        case 6:
                        case 7:
                            spaceV_Hor = 30; break;
                        case 8:
                        case 9:
                            spaceV_Hor = 10; break;
                        default:
                            break;
                    }
                    if (handeled.Find(v => v == A) == null && A.Location.X == 0 && A.Location.Y == 0)
                    {
                        A.Location = new Point(S.Location.X, S.Location.Y + S.Size + spaceV_Ver);
                        if (j <= n_adj / 2)
                        {
                            if (S.Location.X - l * (A.Size + spaceV_Hor) > 0)
                            {
                                A.Location = new Point(S.Location.X - l++ * (A.Size + spaceV_Hor), S.Location.Y + S.Size + spaceV_Ver);
                            }
                        }
                        else
                        {
                            if (S.Location.X + r * (A.Size + spaceV_Hor) < main_panel.Width - A.Size)
                            {
                                A.Location = new Point(S.Location.X + r++ * (A.Size + spaceV_Hor), S.Location.Y + S.Size + spaceV_Ver);
                            }
                        }
                    }
                }
                handeled.Add(V[i]);
            }
            insertVtoFrame();
        }
    }
}
