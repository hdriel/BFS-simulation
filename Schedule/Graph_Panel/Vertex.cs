using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Graphs.GraphPanel
{
    public class Vertex
    {
        // Round corner of panels - use as sending this function
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,      // x-coordinate of upper-left corner
            int nTopRect,       // y-coordinate of upper-left corner
            int nRightRect,     // x-coordinate of lower-right corner
            int nBottomRect,    // y-coordinate of lower-right corner
            int nWidthEllipse,  // height of ellipse
            int nHeightEllipse  // width of ellipse
         );
        private static readonly int WIDTH = 70;
        private static readonly int SIZE_IMAGE = 20;

        public Panel v_panel { get; set; }

        public Label lable { get; set; }

        private int size;
        public int Size
        {
            get { return size; }
            set
            {
                size = value;
                v_panel.Width = size;
                v_panel.Height = size;
                try
                {
                    v_panel.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, v_panel.Width, v_panel.Height, v_panel.Width+1, v_panel.Height+1));
                }
                catch { }
            }
        }

        public Point Location {
            get { return v_panel.Location; }
            set {
                v_panel.Location = value;
                drag.Location = new Point(v_panel.Width - SIZE_IMAGE, SIZE_IMAGE);
            }
        }

        private string name;
        public string id {
            get { return name; }
            set
            {
                name = value;
                //lable.Text(name);
                lable.Text = value;
            }
        }

        public Bunifu.Framework.UI.BunifuImageButton drag { get; set; }
        public bool Focus { get; set; }

        public Color focusColor { get; set; }

        private Color color;
        public Color Color {
            get { return color; }
            set {
                color = value;
                v_panel.BackColor = value;
            }
        }

        public Color around { get; set; }

        public List<Edge> edges_in { get; set; }
        public List<Edge> edges_out { get; set; }

        public Vertex(String id, int size = 0)
        {
            v_panel = new Panel();
            Size = size == 0 ? WIDTH: size;
            v_panel.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, v_panel.Width, v_panel.Height, 200, 200));

            lable = new Label();
            lable.Text = id;
            lable.BackColor = Color.Transparent;
            lable.Location = new Point(0,0);
            lable.AutoSize = false;
            lable.Size = new System.Drawing.Size(v_panel.Width, v_panel.Height);
            lable.TextAlign = ContentAlignment.MiddleCenter;
            lable.Font = new Font("Comic Sans MS", 25, FontStyle.Bold);

            v_panel.BackColor = Color.LightSteelBlue;

            this.id = id;

            v_panel.Controls.Add(lable);
            lable.BringToFront();
           
            initImage();
            drag.BringToFront();

            edges_in = new List<Edge>();
            edges_out = new List<Edge>();

            around = Color.Yellow;
            color = Color.White;

            lable.MouseEnter += ((s, e) => {
                drag.Show();
            });
            lable.MouseLeave += ((s, e) => {
                drag.Hide();
            });
            v_panel.MouseEnter += ((s, e) => {
                drag.Show();
            });
            v_panel.MouseLeave += ((s, e) => {
                drag.Hide();
            });
        }
        public Vertex(String id, Vertex connectTo, bool twoside) : this(id)
        {
            AddNeighbor_son(connectTo, twoside);
        }

        private void initImage()
        {
            drag = new Bunifu.Framework.UI.BunifuImageButton();
            drag.BackColor = Color.Transparent;
            drag.Image = new Bitmap(Schedule.Properties.Resources.icons8_drag_black, SIZE_IMAGE, SIZE_IMAGE);
            drag.Size = new System.Drawing.Size(SIZE_IMAGE, SIZE_IMAGE);
            drag.SizeMode = PictureBoxSizeMode.Zoom;
            drag.Location = new Point(v_panel.Width - SIZE_IMAGE, SIZE_IMAGE);
            v_panel.Controls.Add(drag);
            drag.BringToFront();
            drag.Hide();
        }


        public Edge AddNeighbor_son(Vertex v, bool oneWay)
        {
            Edge e = new Edge(this, v, !oneWay);
            edges_out.Add(e);

            Edge ein = new Edge(v, this,  !oneWay);
            v.edges_in.Add(ein);

            if (!oneWay) {
                Edge eout = new Edge(this, v, !oneWay);
                v.edges_out.Add(ein);
            }
            
            return e;
        }
        public Edge AddNeighbor_parent(Vertex v, bool oneWay)
        {
            Edge e = new Edge(v, this, !oneWay);
            edges_in.Add(e);
            
            if (!oneWay)
            {
                Edge eout = new Edge(this, v,  !oneWay);
                v.edges_out.Add(eout);
            }
            
            return e;
        }

        public Edge RemoveNeighbor(Vertex v)
        {
            foreach (Edge E in edges_out)
            {
                if (E.v_to.Equals(v))
                {
                    edges_out.Remove(E);
                    return E;
                }
            }
            return null;
        }
        public Edge UpdateNeighborIn(Vertex v)
        {
            Edge e = new Edge(v, this);
            edges_in.Add(e);
            return e;
        }

        public void addToControl(Control p)
        {
            p.Controls.Add(v_panel);
        }
    }
}
