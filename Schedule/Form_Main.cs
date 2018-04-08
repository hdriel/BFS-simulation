using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Linq;
using Graphs.GraphPanel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Schedule
{
    public partial class Form_Main : Form
    {
        /* To Collaps all press - Ctrl + M + O */
        /* To open all press    - Ctrl + M + L */
        private int MAX_V = 15, MIN_V = 1;
        private Bitmap iconExitRed, iconExitWhite, initHover, initLeave, initChecked, initChange, btnUp_Green, btnUp_White, btnDown_red, btnDown_white, right_left_full, right_left, up_down_full, up_down;
        private bool clickMenu { get; set; }
        private enum STATUS_RUN { DIDNT_START, START, TIMER, MANUAL, NEXT, PREV, PAUSE, RESUME, CHANGE_SPEED, FINISHED };
        private enum SIZE_V { SMALL = 50, NORMAL = 75, LARGE = 100 };
        private Panel[] panels_footer;
        private STATUS_RUN e_state = STATUS_RUN.DIDNT_START;
        private List<string> V_Chars = new List<string>();
        private Label[,] mat;
        private bool[,] mat_checked;
        private TableLayoutPanel tableMat = new TableLayoutPanel();
        private TableLayoutPanel tableResult = new TableLayoutPanel();
        private Graph graph;
        private Graph graphTree;
        private enum SLEEP {SLOW = 1000, MADIUM = 500, FAST = 50 , STOP = -1};
        private Label[,] mat_result;
        private static int SleepTimeAutoRun;

        bool didntFinished { get; set; }
        bool isOnAuto { get; set; }

        /* Ctor of the Form */
        public Form_Main()
        {
            InitializeComponent();
            
            this.RightToLeftLayout = true;

            Bunifu.Framework.UI.BunifuFlatButton ext = btn_exit_out;
            iconExitRed = new Bitmap(Schedule.Properties.Resources.ShutdownRed, btn_exit_out.Iconimage.Size.Width, btn_exit_out.Iconimage.Size.Height);
            iconExitWhite = new Bitmap(Schedule.Properties.Resources.shutdown, btn_exit_out.Iconimage.Size.Width, btn_exit_out.Iconimage.Size.Height);
            initHover = new Bitmap(Schedule.Properties.Resources.icons8_thumbs_up_filled__1_, btn_init.Iconimage.Size.Width, btn_init.Iconimage.Size.Height);
            initLeave = new Bitmap(Schedule.Properties.Resources.icons8_thumbs_up__1_, btn_init.Iconimage.Size.Width, btn_init.Iconimage.Size.Height);
            initChecked = new Bitmap(Schedule.Properties.Resources.icons8_thumbs_up_filled_checked, btn_init.Iconimage.Size.Width, btn_init.Iconimage.Size.Height);
            initChange = new Bitmap(Schedule.Properties.Resources.icons8_thumbs_up_orange, btn_init.Iconimage.Size.Width, btn_init.Iconimage.Size.Height);
            btnUp_Green = new Bitmap(Schedule.Properties.Resources.icons8_scroll_up_green, btn_amountV_up.Image.Size.Width, btn_amountV_up.Image.Size.Height);
            btnUp_White = new Bitmap(Schedule.Properties.Resources.icons8_scroll_up, btn_amountV_up.Image.Size.Width, btn_amountV_up.Image.Size.Height);
            btnDown_red = new Bitmap(Schedule.Properties.Resources.icons8_scroll_down_red, btn_amountV_down.Image.Size.Width, btn_amountV_down.Image.Size.Height);
            btnDown_white = new Bitmap(Schedule.Properties.Resources.icons8_scroll_down, btn_amountV_down.Image.Size.Width, btn_amountV_down.Image.Size.Height);

            right_left_full = new Bitmap(Schedule.Properties.Resources.right_left_white_full, 26, 26);
            right_left = new Bitmap(Schedule.Properties.Resources.right_left_white, 26, 26);
            up_down_full = new Bitmap(Schedule.Properties.Resources.up_down_white_full, 26, 26);
            up_down = new Bitmap(Schedule.Properties.Resources.up_down_white, 26, 26);

            panels_footer = new Panel[3] { panel_algo_footer_start, panel_algo_footer_manual, panel_algo_footer_automat };
            panel_slide_center.Visible = false;
            lbl_title_init.Visible = false;
            
            HandlerState_visiblePanel(panel_algo_footer_start);
            panel_subtitle_type.Paint += new PaintEventHandler(PaintBorder);
            panel_subtitle_mainV.Paint += new PaintEventHandler(PaintBorder);
            panel_subtitle_mat.Paint += new PaintEventHandler(PaintBorder);
            panel_subtitle_amount.Paint += new PaintEventHandler(PaintBorder);
            btn_init.OnHovercolor = Color.Transparent;
            btn_init.Activecolor = Color.Transparent;
            btn_init.Normalcolor = Color.Transparent;
            e_state = STATUS_RUN.DIDNT_START;
            prevSelectedCombobox = "S";
            
            initTableAlgo();
            panel_algo_footer.Visible = false;
            btn_resetLocationsV.Visible = false;
            foreachV_init_all = new List<Vertex>();
            panel_graph_panel.Visible = true;
            showTreeGraph = false;
            graphTree = null;
            panel_tree_panel.Visible = false;

            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            panel_slidemenu.Width = 50;
            setResizePanels();
            pictureBox_hr_output_graph.Visible = false;
            pictureBox_hr_algo_graph.Visible = false;
        }

        private void PaintBorder(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            int BorderWidth = 3;
            int space = 10;
            using (Pen p = new Pen(Color.White, BorderWidth))
            using (var gp = new GraphicsPath())
            {
                float halfPenWidth = BorderWidth / 2;
                var borderRect = new RectangleF(space, space * 2, panel.Width - space * 2, panel.Height - space * 2 - 10);
                gp.AddRectangle(borderRect);
                e.Graphics.DrawPath(p, gp);
            }
        }
        
        /* Handler the state of the running algorithm */
        private void HandlerState_visiblePanel(Panel p)
        {
            panel_algo_footer_start.Visible = false;
            panel_algo_footer_automat.Visible = false;
            panel_algo_footer_manual.Visible = false;

            if(p!= null) { 
                p.Visible = true;
                panel_algo_footer.Height = p.Height;
            }
        }
        private void HandlerState(Bunifu.Framework.UI.BunifuThinButton2 btn)
        {
            if (btn == btn_start)
            {
                HandlerState_visiblePanel(panel_algo_footer_manual);
            }
            else if (btn == btn_auto)
            {
                HandlerState_visiblePanel(panel_algo_footer_automat);
            }
            else if (btn == btn_manual)
            {
                HandlerState_visiblePanel(panel_algo_footer_manual);
            }
        }

        /* the button to open and close the slider*/
        private void btn_menu_Click(object sender, EventArgs e)
        {
            clickMenuSide();
        }
        private void clickMenuSide()
        {
            clickMenu = !clickMenu;
            if (clickMenu)
            {
                panel_slidemenu.Visible = false;
                panel_slidemenu.Width = this.Width/4; // 300
                PanelAnimator.ShowSync(panel_slidemenu);
                panel_slide_center.Visible = true;
                lbl_title_init.Visible = true;
                panel_subtitle_mat_table.Width = this.Width / 4 - 40;
                panel_subtitle_mat_table.Height = panel_subtitle_mat.Height - lbl_subtitle_mat.Height - 30;

                checkbox_typeGraph.Location = new Point(panel_subtitle_type.Width - 50, checkbox_typeGraph.Location.Y);
                lbl_typeGraph.Location = new Point(panel_subtitle_type.Width - checkbox_typeGraph.Width - lbl_typeGraph.Width - 50, lbl_typeGraph.Location.Y);
                lbl_subtitle_type.Location = new Point(panel_subtitle_type.Width - lbl_subtitle_type.Width - 20, lbl_subtitle_type.Location.Y);

                lbl_subtitle_amount.Location = new Point(panel_subtitle_amount.Width - lbl_subtitle_amount.Width - 20, lbl_subtitle_amount.Location.Y);
                textbox_amountV.Location = new Point(panel_subtitle_amount.Width / 2 - textbox_amountV.Width / 2, textbox_amountV.Location.Y);
                btn_amountV_up.Location = new Point(textbox_amountV.Location.X + textbox_amountV.Width + 10, btn_amountV_up.Location.Y);
                btn_amountV_down.Location = new Point(textbox_amountV.Location.X - btn_amountV_down.Width - 10, btn_amountV_down.Location.Y);

                lbl_subtitle_mainV.Location = new Point(panel_subtitle_mainV.Width - lbl_subtitle_mainV.Width - 20, lbl_subtitle_mainV.Location.Y);
                comboBox_listV.Location = new Point(panel_subtitle_mainV.Width / 2 - comboBox_listV.Width / 2, comboBox_listV.Location.Y);

                lbl_subtitle_mat.Location = new Point(panel_subtitle_mat.Width - lbl_subtitle_mat.Width - 20, lbl_subtitle_mat.Location.Y);

                btn_exit_out.Text = "יציאה";
            }
            else
            {
                panel_slidemenu.Visible = false;
                panel_slidemenu.Width = 50;
                PanelAnimator.ShowSync(panel_slidemenu);
                panel_slide_center.Visible = false;
                lbl_title_init.Visible = false;
                btn_exit_out.Text = "";
            }
        }

        /* Label/Icon buttons- click on the slide */
        private void btn_exit_Click(object sender, EventArgs e)
        {
            //saveData();
            this.Close();
        }

        /* The link to the site of the icon8 , by the requirements on this site for there icons use*/
        private void link_icon8_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://icons8.com/");
        }

        /* clear the panels from the main panel */
        private void enablePanel(Bunifu.Framework.UI.BunifuFlatButton btn = null)
        {
            /*
            panelMain.Controls.Remove(panel_page1);
            panelMain.Controls.Remove(panel_page2);
            panelMain.Controls.Remove(panel_page3);
            panelMain.Controls.Remove(...);

            if (btn == btn_home)
            {
                lbl_title_selected.Text = "דף ראשי";
            }
            else if (btn == btn_courses)
            {
                lbl_title_selected.Text = "רשימת קורסים";
            }
            else if (btn == btn_import)
            {
                lbl_title_selected.Text = "ייבוא";
            }
            else if (btn == btn_save)
            {
                lbl_title_selected.Text = "שמירה / טעינה";
            }
            else if (btn == btn_setting)
            {
                lbl_title_selected.Text = "הגדרות";
            }
            else if (btn == btn_export)
            {
                lbl_title_selected.Text = "ייצוא";
            }
            else if (btn == btn_listCourses2)
            {
                lbl_title_selected.Text = "רשימת שיעורים";
            }
            */
        }

        /* the Exit Label on the slide */
        private void btn_exit_out_Click(object sender, EventArgs e)
        {
            //saveData();
            this.Close();
        }
        private void btn_exit_MouseLeave(object sender, EventArgs e)
        {
            btn_exit.BackColor = Color.Transparent;
        }
        private void btn_exit_MouseEnter(object sender, EventArgs e)
        {
            btn_exit.BackColor = Color.Red;
        }
        private void btn_exit_out_MouseLeave(object sender, EventArgs e)
        {
            btn_exit_out.Iconimage = iconExitWhite;
            btn_exit_out.ForeColor = Color.Silver;
        }

        /* the Minimize button on the left-top corder */
        private void btn_minimize_MouseEnter(object sender, EventArgs e)
        {
            btn_minimize.BackColor = Color.DarkSlateBlue;
        }
        private void btn_minimize_MouseLeave(object sender, EventArgs e)
        {
            btn_minimize.BackColor = Color.Transparent;
        }
        private void btn_minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        /* the Exit button on the left-top corder */
        private void btn_exit_out_MouseEnter(object sender, EventArgs e)
        {
            btn_exit_out.Iconimage = iconExitRed;
            btn_exit_out.ForeColor = Color.Red;
        }
        private void btn_exit_out_MouseHover(object sender, EventArgs e)
        {
            btn_exit_out_MouseEnter(sender, e);
        }

        /* buttons footer algorithm */
        private void btn_start_Click(object sender, EventArgs e)
        {
            initResetData();
            HandlerState(btn_start);
        }
        private void btn_next_Click(object sender, EventArgs e)
        {
            HandlerState(btn_next);
            HighlightNextLine();
        }
        private void initResetData()
        {
            isOnAuto = false;
            intoLoop = false;
            didntFinished = true;
            RunNumberLine = 0;
            changePositionHighlight(RunNumberLine, true, false);
            iterate_foreach_init_all = 0;
            arrow.Visible = true;
            foreachV_init_all = null;
            ListItemsBeforeQueue = null;
            ListItemsAfterQueue = null;
            QueueItems = null;
            Queue = null;
            CurrentV = null;
            nextAdjV = null;
            indexVofAdj = 0;
            panel_output_queue.Controls.Clear();
            initResultTable();
            foreach (Vertex v in graph.V)
            {
                v.Color = VertexAlgo.ColorDefault;
                v.lable.ForeColor = VertexAlgo.ColorFinished;
            }
            foreach (Edge e in graph.E)
            {
                e.color = VertexAlgo.EdgeDefault;
            }
            graph.main_panel.Refresh();
        }
        private void btn_auto_Click(object sender, EventArgs e)
        {
            HandlerState(btn_auto);
            SleepTimeAutoRun = (int)SLEEP.STOP;
            isOnAuto = true;
            enable_btn_speed(0);
            RunAuto();
        }
        private void btn_manual_Click(object sender, EventArgs e)
        {
            SleepTimeAutoRun = (int)SLEEP.STOP;
            isOnAuto = false;
            HandlerState(btn_manual);
        }

        /* Method to sleep for MS time , and dont stack the program - is a Task, and not a thread*/
        private Task SleepAsync()
        {
            return Task.Run(() =>
            {
                if (SleepTimeAutoRun != (int)SLEEP.STOP)
                {
                    Thread.Sleep(SleepTimeAutoRun);
                }
                else
                {
                    Thread.Sleep(1000);
                }
            });
        }
        private async void RunAuto()
        {
            while (didntFinished && isOnAuto)
            {
                await SleepAsync();
                if(SleepTimeAutoRun != (int)SLEEP.STOP)
                    HighlightNextLine();
            }
        }
        private bool[] btn_speed = { false, false, false, false };
        private void btn_speed_stop_Click(object sender, EventArgs e)
        {
            SleepTimeAutoRun = (int)SLEEP.STOP;
            enable_btn_speed(0);
        }
        private void btn_speed_slow_Click(object sender, EventArgs e)
        {
            SleepTimeAutoRun = (int)SLEEP.STOP;
            SleepTimeAutoRun = (int)SLEEP.SLOW;
            enable_btn_speed(1);
        }
        private void btn_speed_medium_Click(object sender, EventArgs e)
        {
            SleepTimeAutoRun = (int)SLEEP.STOP;
            SleepTimeAutoRun = (int)SLEEP.MADIUM;
            enable_btn_speed(2);
        }
        private void btn_speed_fast_Click(object sender, EventArgs e)
        {
            SleepTimeAutoRun = (int)SLEEP.STOP;
            SleepTimeAutoRun = (int)SLEEP.FAST;
            enable_btn_speed(3);
        }
        private void btn_speed_click(Bunifu.Framework.UI.BunifuThinButton2 btn)
        {
            Color clr_back, clr_fore;
            clr_back = btn.ActiveForecolor; 
            clr_fore = btn.IdleForecolor;

            btn.ActiveForecolor = clr_fore;
            btn.IdleFillColor = clr_fore;

            btn.ActiveFillColor = clr_back;
            btn.ActiveLineColor = clr_back;
            btn.ForeColor = clr_back;
            btn.IdleForecolor = clr_back;
            btn.IdleLineColor = clr_back;
        }
        private void enable_btn_speed(int i = 0)
        {
            if (i >= 0 && i < btn_speed.Length && !btn_speed[i])
            {
                btn_speed[i] = true;
                switch (i)
                {
                    case 0: btn_speed_click(btn_speed_stop); break;
                    case 1: btn_speed_click(btn_speed_slow); break;
                    case 2: btn_speed_click(btn_speed_medium); break;
                    case 3: btn_speed_click(btn_speed_fast); break;
                    default: break;
                }
                for (int j = 0; j < btn_speed.Length; j++)
                {
                    if (j != i && btn_speed[j])
                    {
                        btn_speed[j] = false;
                        switch (j)
                        {
                            case 0: btn_speed_click(btn_speed_stop); break;
                            case 1: btn_speed_click(btn_speed_slow); break;
                            case 2: btn_speed_click(btn_speed_medium); break;
                            case 3: btn_speed_click(btn_speed_fast); break;
                            default: break;
                        }
                    }
                    
                }
            }
        }
        /* Slide menu option of the initilization data */
        private void btn_init_MouseHover(object sender, EventArgs e)
        {
            switch (e_state)
            {
                case STATUS_RUN.DIDNT_START:
                case STATUS_RUN.FINISHED:
                    btn_init.Iconimage = initHover;
                    break;
                default:
                    break;
            }
        }
        private void checkbox_typeGraph_OnChange(object sender, EventArgs e)
        {
            lbl_typeGraph.Text = checkbox_typeGraph.Checked ? "גרף מכוון" : "גרף לא מכוון";
            if (mat != null)
            {
                mirrorMat(ref mat_checked);
                int n = mat.GetLength(0);
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (i != 0 && j != 0)
                        {
                            if (!checkbox_typeGraph.Checked)
                            {
                                if (i >= j)
                                {
                                    mat[i, j].Enabled = false;
                                    mat[i, j].BackColor = Color.Gray;
                                }
                            }
                            else
                            {
                                if (i >= j)
                                {
                                    mat[i, j].Enabled = true;
                                    mat[i, j].BackColor = mat_checked[i, j] ? Color.SeaGreen : Color.Tomato;
                                }
                            }
                        }
                    }
                }
            }
        }

        /* buttons up and down of the amount vertexes */
        private void btn_amountV_up_Click(object sender, EventArgs e)
        {
            try
            {
                int n = 0;
                if (!(textbox_amountV.Text.Equals("N")))
                {
                    n = Convert.ToInt32(textbox_amountV.Text);
                }
                if (n + 1 <= MAX_V)
                {
                    textbox_amountV.Text = (n + 1).ToString();
                }
            }
            catch { }
        }
        private void btn_amountV_up_MouseHover(object sender, EventArgs e)
        {
            btn_amountV_up.Image = btnUp_Green;
        }
        private void btn_amountV_up_MouseLeave(object sender, EventArgs e)
        {
            btn_amountV_up.Image = btnUp_White;
        }
        private void btn_amountV_down_Click(object sender, EventArgs e)
        {
            try
            {
                int n = Convert.ToInt32(textbox_amountV.Text);
                if (n - 1 >= MIN_V)
                {
                    textbox_amountV.Text = (n - 1).ToString();
                }
                else
                {
                    textbox_amountV.Text = "N";
                }
            }
            catch { }
        }
        private void btn_amountV_down_MouseHover(object sender, EventArgs e)
        {
            btn_amountV_down.Image = btnDown_red;
        }
        private void btn_amountV_down_MouseLeave(object sender, EventArgs e)
        {
            btn_amountV_down.Image = btnDown_white;
        }
        private void textbox_amountV_OnValueChanged(object sender, EventArgs e)
        {
            int n = 0;
            try
            {
                if (textbox_amountV.Text.Length > 0)
                    n = Convert.ToInt32(textbox_amountV.Text);
            }
            catch
            {
                return;
            }
            initListVertexesNames(n);
            initDataAtComboBox(V_Chars);
        }

        /* button of the init data on the slide menu */
        private void btn_init_Click(object sender, EventArgs e)
        {
            if (mat == null) { return; }
            e_state = STATUS_RUN.START;
            btn_init.Iconimage = initChecked;
            clickMenuSide();
            if (graph != null)
            {
                graph.ClearPanel();
            }
            graph = new Graph(ref panel_graph_panel, V_Chars, mat_checked, checkbox_typeGraph.Checked, (int)SIZE_V.NORMAL);
            VertexAlgo.graph = graph;

            if(graphTree != null)
            {
                graphTree.VetexCotrols(false);
            }
            btn_switch_graph.Visible = false;
            showTreeGraph = false;
            panel_tree_panel.Visible = false;
            panel_tree_panel.SendToBack();
            graphTree = null;

            panel_graph_panel.Visible = true;
            panel_graph_panel.BringToFront();
            graph.VetexCotrols(true);

            panel_algo_footer.Visible = true;
            didntFinished = true;
            HandlerState_visiblePanel(panel_algo_footer_start);
            initResetData();
            pictureBox_hr_output_graph.Visible = true;
            pictureBox_hr_algo_graph.Visible = true;
        }
        private void btn_init_MouseEnter(object sender, EventArgs e)
        {
            switch (e_state)
            {
                case STATUS_RUN.DIDNT_START:
                case STATUS_RUN.FINISHED:
                    btn_init.Iconimage = initHover;
                    break;
                default:
                    break;
            }
        }
        private void btn_init_MouseLeave(object sender, EventArgs e)
        {
            switch (e_state)
            {
                case STATUS_RUN.DIDNT_START:
                case STATUS_RUN.FINISHED:
                    btn_init.Iconimage = initLeave;
                    break;
                default:
                    break;
            }


        }

        /* combobox of the main V */
        private string prevSelectedCombobox { get; set; }
        private void comboBox_listV_SelectedIndexChanged(object sender, EventArgs e)
        {
            int si = comboBox_listV.SelectedIndex;
            if (mat != null && mat.GetLength(0) > 2)
            {
                // return to the start
                swichRowCol(prevSelectedCombobox, "S", false);
                swichRowCol("S", prevSelectedCombobox, true);

                string B = comboBox_listV.Text, S = mat[0, 1].Text;
                swichRowCol(S, B, false);
                swichRowCol(B, S, true);

                prevSelectedCombobox = comboBox_listV.Text;

            }
        }
        private void initListVertexesNames(int n)
        {
            if (n > MAX_V) { MessageBox.Show("אתה יכול להציג מקסימום 15 קודקודים וגם זה יותר מידיי כבד לגרפיקה פה"); return; }
            if (n < MIN_V)
            {
                V_Chars.Clear();
                return;
            }
            V_Chars.Clear();
            V_Chars.Add("S");
            int unicode = 65; // ASCII 'A'
            for (int i = 0; i < n - 1; i++)
            {
                char character = (char)(i + unicode);
                V_Chars.Add((character).ToString());
            }
        }
        private void initDataAtComboBox(List<String> items)
        {
            string text = comboBox_listV.Text;
            BindingList<string> bindinglist = new BindingList<string>(items);
            BindingSource bSource = new BindingSource();
            bSource.DataSource = bindinglist;
            comboBox_listV.DataSource = bSource;
            comboBox_listV.Text = text;

            initMatNeiboursTable(items);
        }

        /* Function for the matrix bit and lable */
        private void initMat(List<String> V, Label[,] temp, bool[,] temp_checked)
        {
            if (V.Count <= 0)
            {
                panel_subtitle_mat_table.Controls.Remove(tableMat);
                try
                {
                    tableMat.GetControlFromPosition(0, 0).BackColor = Color.Transparent;
                }
                catch { }
                tableMat.Controls.Clear();
            }
            int n = V.Count + 1;
            mat = new Label[n, n];
            mat_checked = new bool[n, n];
            for (int i = 0, r = 0, c = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Label l = new Label();
                    l.Font = new Font("Guttman Kav", 10, FontStyle.Bold);
                    l.ForeColor = Color.White;
                    l.Dock = DockStyle.Fill;
                    l.TextAlign = ContentAlignment.MiddleCenter;
                    l.Margin = new Padding(0);
                    mat[i, j] = l;
                    if (i == 0 && j == 0)
                        l.Text = "";
                    else if (i == 0 && j != 0)
                    {
                        if (c < V.Count)
                            l.Text = V[c++];
                    }
                    else if (i != 0 && j == 0)
                    {
                        if (r < V.Count)
                            l.Text = V[r++];
                    }
                    else
                    {
                        l.Text = "O";
                        l.BackColor = Color.Tomato;
                    }

                    if (i == 0 || j == 0)
                    {
                        l.BackColor = Color.Yellow;
                        l.ForeColor = Color.Black;
                    }

                    mat_checked[i, j] = false;
                    if (temp != null && i < temp.GetLength(0) && j < temp.GetLength(1))
                    {
                        l.Text = temp[i, j].Text;
                        mat_checked[i, j] = temp_checked[i, j];
                        if (i != 0 && j != 0)
                            l.BackColor = mat_checked[i, j] ? Color.SeaGreen : Color.Tomato;
                    }
                    mat[i, j] = l;

                    if (i != 0 && j != 0)
                    {
                        if (!checkbox_typeGraph.Checked)
                        {
                            if (i >= j)
                            {
                                mat[i, j].Enabled = false;
                                mat[i, j].BackColor = Color.Gray;
                            }
                        }
                        else
                        {
                            if (i >= j)
                            {
                                mat[i, j].Enabled = true;
                                mat[i, j].BackColor = mat_checked[i, j] ? Color.SeaGreen : Color.Tomato;
                            }
                        }
                    }
                }
            }
        }
        private void initMatNeiboursTable(List<String> V)
        {
            initMat(V, mat, mat_checked);
            tableMat.Controls.Clear();
            tableMat = new TableLayoutPanel();
            tableMat.Dock = DockStyle.Fill;
            tableMat.ColumnCount = mat.GetLength(0);
            tableMat.RowCount = mat.GetLength(1);
            tableMat.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            int presenceWidth = 100 / mat.GetLength(0);

            panel_subtitle_mat_table.Controls.Clear();

            for (int i = 0; i < tableMat.RowCount; i++)
            {
                for (int j = 0; j < tableMat.ColumnCount; j++)
                {
                    ColumnStyle cs = new ColumnStyle() { SizeType = SizeType.Percent, Width = presenceWidth };
                    tableMat.ColumnStyles.Add(cs);
                    RowStyle rs = new RowStyle() { SizeType = SizeType.Percent, Height = presenceWidth };
                    tableMat.RowStyles.Add(rs);
                    tableMat.Controls.Add(mat[i, j]);
                }
            }
            tableMat.Location = new Point(50, lbl_subtitle_mat.Height + 5);

            foreach (Control c in tableMat.Controls.OfType<Label>())
            {
                c.MouseClick += ((s, e) =>
                {
                    Label l = (Label)s;
                    //if (e.Button == MouseButtons.Right)
                    {
                        for (int i = 0; i < mat.GetLength(0); i++)
                        {
                            for (int j = 0; j < mat.GetLength(1); j++)
                            {
                                if (mat[i, j] == l)
                                {
                                    if (i != 0 && j != 0)
                                    {
                                        mat_checked[i, j] = !mat_checked[i, j];
                                        l.Text = mat_checked[i, j] ? "X" : "O";
                                        l.BackColor = mat_checked[i, j] ? Color.SeaGreen : Color.Tomato;
                                    }
                                }
                            }
                        }
                        if (!checkbox_typeGraph.Checked) mirrorMat(ref mat_checked);
                    }

                });
            }
            tableMat.Margin = new Padding(0);
            panel_subtitle_mat_table.Controls.Add(tableMat);
            tableMat.AutoScroll = true;
        }
        private void mirrorMat(ref bool[,] mat_checked)
        {
            if (mat_checked != null)
            {
                int n = mat_checked.GetLength(0);
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        mat_checked[j, i] = mat_checked[i, j];
                        mat[j, i].Text = mat[i, j].Text;
                    }
                }
            }
        }
        private void swichRowCol(string S, string B, bool row)
        {
            if (mat != null && mat.GetLength(0) > 2)
            {
                List<string> l_1 = new List<string>();
                List<string> l_2 = new List<string>();
                int n = mat.GetLength(0);
                int i1 = 1; // first column - S 
                if (row)
                {
                    for (int r = 0; r < n; r++)
                    {
                        l_1.Add(mat[i1, r].Text);
                    }
                    int i2 = -1; // secend column - C 
                    for (int c = 2; c < n; c++)
                    {
                        if (mat[0, c].Text.Equals(B))
                        {
                            i2 = c;
                            for (int r = 0; r < n; r++)
                            {
                                l_2.Add(mat[i2, r].Text);
                            }
                            break;
                        }
                    }
                    if (l_1.Count == l_2.Count && l_1.Count == n)
                        for (int r = 0; r < n; r++)
                        {
                            mat[i1, r].Text = l_2[r];
                            mat[i2, r].Text = l_1[r];
                            if (r != 0)
                            {
                                mat_checked[i1, r] = mat[i1, r].Text.Equals("X");
                                mat[i1, r].BackColor = mat_checked[i1, r] ? Color.SeaGreen : Color.Tomato;

                                mat_checked[i2, r] = mat[i2, r].Text.Equals("X");
                                mat[i2, r].BackColor = mat_checked[i2, r] ? Color.SeaGreen : Color.Tomato;

                                if (!checkbox_typeGraph.Checked)
                                {
                                    if (r <= i1)
                                    {
                                        mat[i1, r].BackColor = Color.Gray;
                                    }
                                    if (r <= i2)
                                    {
                                        mat[i2, r].BackColor = Color.Gray;
                                    }

                                }
                            }
                        }
                }
                else
                {
                    for (int r = 0; r < n; r++)
                    {
                        l_1.Add(mat[r, i1].Text);
                    }
                    int i2 = -1; // secend column - C 
                    for (int c = 2; c < n; c++)
                    {
                        if (mat[0, c].Text.Equals(B))
                        {
                            i2 = c;
                            for (int r = 0; r < n; r++)
                            {
                                l_2.Add(mat[r, i2].Text);
                            }
                            break;
                        }
                    }
                    if (l_1.Count == l_2.Count && l_1.Count == n)
                        for (int r = 0; r < n; r++)
                        {
                            mat[r, i1].Text = l_2[r];
                            mat[r, i2].Text = l_1[r];
                            if (r != 0)
                            {
                                mat_checked[r, i1] = mat[r, i1].Text.Equals("X");
                                mat[r, i1].BackColor = mat_checked[r, i1] ? Color.SeaGreen : Color.Tomato; ;

                                mat_checked[r, i2] = mat[r, i2].Text.Equals("X");
                                mat[r, i2].BackColor = mat_checked[r, i2] ? Color.SeaGreen : Color.Tomato; ;

                                if (!checkbox_typeGraph.Checked)
                                {
                                    if (i1 <= r)
                                    {
                                        mat[r, i1].BackColor = Color.Gray;
                                    }
                                    if (i2 <= r)
                                    {
                                        mat[r, i2].BackColor = Color.Gray;
                                    }

                                }
                            }
                        }
                }

            }
        }

        /* large the Vertexes of Small them */
        private void btn_zoom_outV_Click(object sender, EventArgs e)
        {
            if (graph != null) graph.ChangeSize(45);
        }
        private void btn_zoom_inV_Click(object sender, EventArgs e)
        {
            if (graph != null) graph.ChangeSize(70);
        }
        private void btn_resetLocationsV_Click(object sender, EventArgs e)
        {
            btn_resetLocationsV.Visible = false;
            panel_algo_footer.Visible = true;
            arrow.Visible = true;

            if(graphTree != null)
            {
                graphTree.VetexCotrols(false);
            }
            btn_switch_graph.Visible = false;
            showTreeGraph = false;
            panel_tree_panel.Visible = false;
            panel_tree_panel.SendToBack();
            graphTree = null;

            HandlerState_visiblePanel(panel_algo_footer_start);
            initResetData();
        }

        /* Table of the algorithm to run */
        private int RunNumberLine { get; set; }
        private List<Label> lblLineAlgo { get; set; }
        private Bunifu.Framework.UI.BunifuImageButton arrow { get; set; }
        private void initTableAlgo()
        {
            lblLineAlgo = new List<Label>();
            table_algo.Controls.Clear();

            arrow = new Bunifu.Framework.UI.BunifuImageButton();
            arrow.Image = new Bitmap(Schedule.Properties.Resources.icons8_right_arrow_debuger, 52, 50);
            arrow.SizeMode = PictureBoxSizeMode.CenterImage;
            arrow.BackColor = Color.Transparent;
            arrow.Parent = table_algo;
            arrow.Visible = false;

            TableLayoutPanelCellPosition p = new TableLayoutPanelCellPosition(0, 0);
            table_algo.SetCellPosition(arrow, p);

            for (int r = 0; r < table_algo.RowCount; r++)
            {
                Label l = new Label()
                {
                    BackColor = Color.Transparent,
                    Dock = DockStyle.Fill,
                    Font = new Font("Levenim MT", 14, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = Color.White,
                    Text = getTextAlgoByLine(r)
                };
                switch (r)
                {
                    case 5:
                    case 20:
                        l.Font = new Font("Segoe UI", 14, FontStyle.Bold);
                        break;
                }
                table_algo.Controls.Add(l, 1, r);
                lblLineAlgo.Add(l);
            }
        }
        private string getTextAlgoByLine(int r)
        {
            switch (r)
            {
                case 0: return "BFS (G, S)";
                case 1: return "{";
                case 2: return "    for each v ∈ V";
                case 3: return "    {";
                case 4: return "         d[ v ] ← ∞";
                case 5: return "           π[ v ] ← NIL";
                case 6: return "         Color[ v ] ← WHITE";
                case 7: return "    }";
                case 8: return "    d[ s ] ← 0";
                case 9: return "    Q ← Ø";
                case 10: return "    color[ s ] ← GARY";
                case 11: return "    Enqueue( Q , s)";
                case 12: return "    while ( Q ≠ Ø)";
                case 13: return "    {";
                case 14: return "        u ← Dequeue( Q )";
                case 15: return "        for each v ∈ Adj[ u ]";
                case 16: return "        {";
                case 17: return "             if( color[v] == WHITE)";
                case 18: return "             {";
                case 19: return "                 d[ v ] ← d[ u ] + 1";
                case 20: return "                     π[ v ] ← u";
                case 21: return "                 color[v] ← GRAY";
                case 22: return "                 Enqueue( Q, v)";
                case 23: return "             }";
                case 24: return "        }";
                case 25: return "        color[ u ] ← BLACK";
                case 26: return "    }";
                case 27: return "}";
                default: return "";
            }
        }
        private void HighlightNextLine()
        {
            if (RunNumberLine >= 0 && RunNumberLine <= 26)
            {
                RunNumberLine = algorithm_run_line(RunNumberLine, true);
                RunNumberLine = changePositionHighlight(RunNumberLine, true, false);
            }
            else
            {
                RunNumberLine = changePositionHighlight(RunNumberLine, true, true);
            }
            graph.main_panel.Refresh();
            if (RunNumberLine >= 27)
            {
                btn_resetLocationsV.Visible = true;
                HandlerState_visiblePanel(null);
                panel_algo_footer.Visible = false;
                arrow.Visible = false;
                initTreeGraph();
                return;
            }
            if (RunNumberLine < lblLineAlgo.Count)
                lblLineAlgo[RunNumberLine].ForeColor = Color.Yellow;
        }
        private void HighlightPrevLine()
        {
            if (RunNumberLine >= 2 && RunNumberLine <= 26)
            {
                RunNumberLine = algorithm_run_line(RunNumberLine, false);
                RunNumberLine = changePositionHighlight(RunNumberLine, false, false);
            }
            else
            {
                RunNumberLine = changePositionHighlight(RunNumberLine, false, true);
            }
            graph.main_panel.Refresh();
            if (RunNumberLine >= 0)
                lblLineAlgo[RunNumberLine].ForeColor = Color.Yellow;
        }
        private int changePositionHighlight(int r, bool addByOne, bool referOne = true)
        {
            TableLayoutPanelCellPosition p = table_algo.GetCellPosition(arrow);
            for (int i = 0; i < lblLineAlgo.Count; i++)
            {
                lblLineAlgo[i].ForeColor = Color.White;
            }

            if (referOne)
            {
                p.Column = 0;
                if (addByOne)
                {
                    p.Row++;
                    table_algo.SetCellPosition(arrow, p);


                    if (!(p.Row >= 0 && p.Row < table_algo.RowCount))
                    {
                        didntFinished = false;
                    }
                }
                else
                {
                    p.Row--;
                    if (p.Row >= 0)
                    {
                        table_algo.SetCellPosition(arrow, p);
                    }
                    if (!(p.Row >= 0 && p.Row < table_algo.RowCount))
                    {
                        RunNumberLine = -1;
                        didntFinished = false;
                    }
                }
            }
            else
            {
                p.Row = r;
                table_algo.SetCellPosition(arrow, p);
                RunNumberLine = r;
            }
            return p.Row;
        }

        /* the Output panel  */
        private void initResultMat()
        {
            if (mat == null)
            {
                panel_output_result.Controls.Remove(tableResult);
                try
                {
                    tableResult.Controls.Clear();
                    tableResult.GetControlFromPosition(0, 0).BackColor = Color.Transparent;
                }
                catch { }
                return;
            }
            int rows = 4;
            int cols = mat.GetLength(1);
            mat_result = new Label[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Label l = new Label();
                    l.Font = new Font("Levenim MT", 14, FontStyle.Bold);
                    l.ForeColor = Color.White;
                    l.Dock = DockStyle.Fill;
                    l.TextAlign = ContentAlignment.MiddleCenter;
                    l.Margin = new Padding(0);
                    if (i == 0)
                    {
                        l.Text = mat[0, j].Text;
                    }
                    else if (j == 0)
                    {
                        switch (i)
                        {
                            case 0: l.Text = ""; break;
                            case 1: l.Text = "d"; break;
                            case 2: l.Text = "π"; l.Font = new Font("Segoe UI", 14, FontStyle.Bold); break;
                            case 3: l.Text = "color"; break;
                            default: break;
                        }
                    }
                    else if (i != 0 && j != 0)
                    {
                        l.Text = ".";
                    }
                    mat_result[i, j] = l;
                }
            }
            VertexAlgo.matResult = mat_result;
        }
        private void initResultTable()
        {
            initResultMat();
            tableResult.Controls.Clear();
            tableResult = new TableLayoutPanel();
            tableResult.Dock = DockStyle.Fill;
            tableResult.ColumnCount = mat_result.GetLength(1);
            tableResult.RowCount = mat_result.GetLength(0);
            tableResult.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            int presenceWidth = 100 / mat_result.GetLength(0);
            int presencHeight = 100 / mat_result.GetLength(1);
            panel_output_result.Controls.Clear();

            for (int i = 0; i < tableResult.RowCount; i++)
            {
                for (int j = 0; j < tableResult.ColumnCount; j++)
                {
                    ColumnStyle cs = new ColumnStyle() { SizeType = SizeType.Percent, Width = presenceWidth };
                    tableResult.ColumnStyles.Add(cs);
                    RowStyle rs = new RowStyle() { SizeType = SizeType.Percent, Height = presencHeight };
                    tableResult.RowStyles.Add(rs);
                    tableResult.Controls.Add(mat_result[i, j]);
                }
            }
            tableResult.Location = new Point(0, 0);

            foreach (Control c in tableResult.Controls.OfType<Label>())
            {
                c.MouseClick += ((s, e) =>
                {
                    Label l = (Label)s;
                    {
                        for (int i = 0; i < mat_result.GetLength(0); i++)
                        {
                            for (int j = 0; j < mat_result.GetLength(1); j++)
                            {
                                if (mat_result[i, j] == l)
                                {
                                    if (i != 0 && j != 0)
                                    {
                                    }
                                }
                                else
                                {

                                }
                            }
                        }
                    }

                });
            }
            tableResult.Margin = new Padding(0);
            panel_output_result.Controls.Add(tableResult);
            tableResult.AutoScroll = true;
        }

        /* the algorithm for running*/
        private Queue<Vertex> Queue { get; set; }
        private Vertex CurrentV { get; set; }
        private Vertex nextAdjV { get; set; }
        private int indexVofAdj { get; set; }
        /*
        private int findIndexMatResultVByID(string id)
        {
            for (int i = 0; i < mat_result.GetLength(1); i++)
            {
                if (mat_result[0, i].Text.Equals(id))
                {
                    return i;
                }
            }
            return -1;
        }
        */
        private bool intoLoop { get; set; }
        private int iterate_foreach_init_all { get; set; }
        private List<Vertex> foreachV_init_all { get; set; }

        /* Object to handel data algorithm */
        private VertexAlgo CurrV { get; set; }
        private VertexAlgo AdjV { get; set; }
        private int algorithm_run_line(int r, bool next)
        {
            if (next)
            {
                switch (r)
                {
                    case 0: return r + 1; // BFS (G,S)
                    case 1: return r + 1; // {
                    case 2: // for each v ∈ V
                        if (iterate_foreach_init_all < graph.V.Count)
                        {
                            CurrV = new VertexAlgo(iterate_foreach_init_all++);
                            CurrV.focus(true, true);
                            return r + 1;
                        }
                        else
                        {
                            iterate_foreach_init_all = 0;
                            return 8;
                        }
                    case 3: // {
                        return r + 1;
                    case 4: //  d[ u ] ← ∞
                        CurrV.setDistanceFromV(null);
                        return r + 1;
                    case 5: // π[ u ] ← NIL
                        CurrV.resetParentV();
                        return r + 1;
                    case 6: // Color[ u ] ← WHITE
                        CurrV.color = Color.White;
                        return r + 1;
                    case 7: // } end - for each 
                        return 2;
                    case 8: //  d[ s ] ← 0
                        VertexAlgo.ClearFocus();
                        CurrV = new VertexAlgo(iterate_foreach_init_all);
                        CurrV.focus(true, true);
                        VertexAlgo.setDistanceRoot();
                        return r + 1;
                    case 9: //    Q ← Ø  >  init Queue
                        Queue = new Queue<Vertex>();
                        initQueueItem();
                        return r + 1;
                    case 10: // color[ s ] ← GARY
                        CurrV.color = VertexAlgo.ColorQueue;
                        return r + 1;
                    case 11: // Enqueue( Q , s) >  insert S to Queue
                        if (Queue == null || Queue.Count > 0)
                            Queue = new Queue<Vertex>();
                        Queue.Enqueue(CurrV.getV());
                        updateQueuePanel(CurrV.getV().id, true);
                        return r + 1;
                    case 12: // while ( Q ≠ Ø)
                        
                        if (Queue.Count != 0)
                            return r + 1;
                        else
                        {
                            if (isOnAuto)
                            { 
                                btn_speed_stop_Click(null, null);
                                didntFinished = false;
                                isOnAuto = false;
                                VertexAlgo.ClearFocus();
                            }
                            return 27;
                        }
                    case 13: // { while
                        return r + 1;
                    case 14: //  u <- dQ
                        CurrentV = Queue.Dequeue();
                        CurrV = new VertexAlgo(CurrentV);
                        CurrV.focus(true, true);
                        return r + 1;
                    case 15: // for each v in adj
                        if(AdjV != null)
                        {
                            Edge e = CurrV.getEdgeToV(AdjV);
                            if(e != null)
                                e.color = VertexAlgo.EdgeFinished;
                        }
                        VertexAlgo.ClearFocus();
                        CurrV.focus(true, true);
                        AdjV = CurrV.getNextAdj();
                        if (AdjV != null)
                        {
                            AdjV.focus(true, false);
                            Edge e = CurrV.getEdgeToV(AdjV);
                            e.color = VertexAlgo.EdgeFocus;
                            return r + 1;
                        }
                        else
                            return 25;
                    case 16: // { of for each adj
                        return r + 1;
                    case 17: // if color == white
                        if (AdjV.color == VertexAlgo.ColorStart)
                            return r + 1;
                        else
                            return 23;
                    case 18: // { if color == white
                        return r + 1;
                    case 19: // d[ v ] ← d[ u ] + 1
                        AdjV.setDistanceFromV(CurrV);
                        return r + 1;
                    case 20: // π[ v ] ← u
                        AdjV.setParentV(CurrV);
                        return r + 1;
                    case 21: // color <- gray
                        AdjV.color = VertexAlgo.ColorQueue;
                        return r + 1;
                    case 22: //eQ v
                        Queue.Enqueue(AdjV.getV());
                        updateQueuePanel(AdjV.getV().id, true);
                        return r + 1;
                    case 23: // } if color == white
                        return r + 1;
                    case 24: // } foreach
                        CurrV.focus(true, true);
                        return 15;
                    case 25: // color <- black
                        updateQueuePanel(null, false);
                        CurrV.color = VertexAlgo.ColorFinished;
                        return r + 1;
                    case 26: // } while
                        return 12;
                    default:
                        return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        /* The Queue panel on the Output panel  */
        List<Label> ListItemsBeforeQueue;
        List<Label> ListItemsAfterQueue;
        Queue<Label> QueueItems;

        private void icon_program_Click(object sender, EventArgs e)
        {
            String message = "כרגע התוכנית היא הדמיה של BFS בלבד, אך אולי ארחיב את זה ליותר אלגוריתמים\n\nתודה שהשתמשת בתוכנה שלי.\nבברכה: הדריאל בנג'ו";
            MessageBox.Show(message, "", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }

        int heightQueue;
        private void initQueueItem()
        {
            panel_output_queue.Controls.Clear();
            heightQueue = panel_output_queue.Height - 10;
            ListItemsBeforeQueue = new List<Label>();
            ListItemsAfterQueue = new List<Label>();
            QueueItems = new Queue<Label>();
            int i = 0;
            foreach (Vertex item in graph.V)
            {
                Label l = new Label();
                l.Text = item.id;
                l.TextAlign = ContentAlignment.MiddleCenter;
                l.AutoSize = false;
                l.Height = heightQueue - 4;
                l.Font = new Font("Segoe UI", 14, FontStyle.Bold);
                l.Width = 50;
                l.BackColor = Color.White;
                l.Location = new Point(2 + (i++*l.Width + 1), 2);
                ListItemsBeforeQueue.Add(l);
                panel_output_queue.Controls.Add(l);
            }
        }
        private Label FindItemQueueByName(string id)
        {
            foreach (Label item in ListItemsBeforeQueue)
            {
                if (item.Text.Equals(id))
                    return item;
            }
            return null;
        }
        private void updateQueuePanel(string id, bool enqueue)
        {
            if(Queue == null)
            {
                panel_output_queue.Controls.Clear();
                return;
            }
            else
            {
                Label lbl = null;
                if (id != null) { 
                    lbl = FindItemQueueByName(id);
                    if (lbl == null) return;
                }
                if (enqueue)
                {
                    ListItemsBeforeQueue.Remove(lbl);
                    QueueItems.Enqueue(lbl);

                    lbl.BackColor = Color.Gray;
                    lbl.ForeColor = Color.White;
                }
                else
                {
                    if (QueueItems.Count > 0)
                    {
                        lbl = QueueItems.Dequeue();
                        ListItemsAfterQueue.Add(lbl);

                        lbl.BackColor = Color.Black;
                        lbl.ForeColor = Color.White;
                    }
                }
                int i = 0;
                int j = 0;
                int startQueue = 0;
                i = 0;
                foreach (Label item in ListItemsBeforeQueue)
                {
                    item.Location = new Point(2 + (i++ * (item.Width + 1)), 2);
                }
                if (ListItemsBeforeQueue.Count > 0)
                    startQueue = ListItemsBeforeQueue[ListItemsBeforeQueue.Count - 1].Location.X + ListItemsBeforeQueue[ListItemsBeforeQueue.Count - 1].Width * 2;
                else
                    startQueue = panel_output_queue.Width / 3;


                i = 0;
                foreach (Label item in QueueItems.ToArray())
                {
                    item.Location = new Point(startQueue + (i++ * (item.Width + 1)), 2);
                }
                
                j = 0;
                foreach (Label item in ListItemsAfterQueue)
                {
                    item.Location = new Point(panel_output_queue.Width - item.Width - (j++ * (item.Width + 1)), 2);
                }
            }
        }

        class VertexAlgo
        {
            public static Color ColorDefault = Color.LightSteelBlue;
            public static Color ColorFinished = Color.Black;
            public static Color ColorStart = Color.White;
            public static Color ColorQueue = Color.Gray;
            public static Color ColorFocus = Color.Yellow;

            public static Color EdgeDefault = Color.White;
            public static Color EdgeFinished = Color.Black;
            public static Color EdgeFocus = Color.Orange;

            public static Label[,] matResult { get; set; }
            public static Graph graph { get; set; }
            private List<VertexAlgo> adj
            {
                get
                {
                    List<VertexAlgo> list = new List<VertexAlgo>();
                    foreach (Edge e in v.edges_out)
                    {
                        list.Add(new VertexAlgo(e.v_to));
                    }
                    return list;
                }
            }

            private int indexAdj { get; set; }


            public string id
            {
                get
                {
                    return v.id;
                }
                set
                {
                    v.id = value;
                }
            }
            public Color color
            {
                get
                {
                    return v.Color;
                }
                set
                {
                    v.Color = value;
                    v.lable.ForeColor = v.Color != ColorStart ? ColorStart : ColorFinished;
                    matResult[3, indexV].ForeColor = v.lable.ForeColor;
                    matResult[3, indexV].BackColor = v.Color == ColorDefault ? Color.Transparent : v.Color;
                    matResult[3, indexV].Text = v.Color == VertexAlgo.ColorStart ? "W" : "G";
                    if (v.Color == VertexAlgo.ColorFinished)
                    {
                        matResult[3, indexV].Text = "B";
                        matResult[3, indexV].BackColor = v.Color == ColorDefault ? Color.Transparent : v.Color;
                        ClearFocus();
                        focus(true, true);
                    }
                    refresh();
                }
            }
            public int indexV { get; set; }
            private Vertex v { get; set; }
            private Label lableInQueue { get; set; }

            public VertexAlgo(int iv)
            {
                indexAdj = 0;
                indexV = iv + 1;
                if (graph == null || matResult == null)
                {
                    throw new Exception("missed static parameter initializetion!!");
                }
                if (iv >= 0 && iv < graph.V.Count)
                    v = graph.V[iv];
                else
                {
                    throw new Exception("בחירת קודקוד " + (indexV - 1) + " הוא מחוץ לתחום!");
                }
                if (indexV <= 0)
                {
                    throw new Exception("didnt found Vertex");
                }
            }
            public VertexAlgo(Vertex v)
            {
                indexAdj = 0;
                this.v = v;
                indexV = findIndexMatResultVByID(v.id);
                if (graph == null || matResult == null)
                {
                    throw new Exception("missed static parameter initializetion!!");
                }
                if (indexV <= 0)
                {
                    throw new Exception("didnt found Vertex");
                }
            }
            public VertexAlgo(string id)
            {
                indexAdj = 0;
                int i = 0;
                this.v = findVertexByID(id, ref i);
                indexV = i;
                if (v == null)
                {
                    throw new Exception("There isn't vertex with the id " + id);
                }
                if (graph == null || matResult == null)
                {
                    throw new Exception("missed static parameter initializetion!!");
                }
                if (indexV <= 0)
                {
                    throw new Exception("didnt found Vertex");
                }
            }

            private int findIndexMatResultVByID(string id)
            {

                for (int i = 0; i < matResult.GetLength(1); i++)
                {
                    if (matResult[0, i].Text.Equals(id))
                    {
                        return i;
                    }
                }
                return -1;
            }
            private Vertex findVertexByID(string id, ref int iv)
            {
                for (int i = 0; i < graph.V.Count; i++)
                {
                    if (graph.V[i].id.Equals(id))
                    {
                        iv = i + 1;
                        return graph.V[i];
                    }
                }
                iv = -1;
                return null;
            }

            public static void setDistanceRoot()
            {
                matResult[1, 1].Text = "0";
            }
            public void setDistanceFromV(VertexAlgo VA)
            {
                try
                {
                    if (VA == null)
                    {
                        matResult[1, indexV].Text = "∞";
                        return;
                    }

                    int dva = VA.getDistance();
                    if (dva != -1)
                    {
                        matResult[1, indexV].Text = (dva + 1).ToString();
                    }
                    else
                    {
                        matResult[1, indexV].Text = "∞";
                    }
                }
                catch
                {
                    matResult[1, indexV].Text = "Error";
                }
            }
            public int getDistance()
            {
                try
                {
                    return Convert.ToInt32(matResult[1, indexV].Text);
                }
                catch
                {
                    return -1;
                }
            }

            public void resetParentV(string str = "NIL")
            {
                matResult[2, indexV].Text = str;
            }
            public void setParentV(VertexAlgo VA)
            {
                matResult[2, indexV].Text = VA == null ? "NIL" : VA.id;
            }
            public VertexAlgo getParentV()
            {
                return new VertexAlgo(matResult[2, indexV].Text);
            }

            public Vertex getV()
            {
                return v;
            }

            public VertexAlgo getNextAdj()
            {
                if (indexAdj < adj.Count)
                {
                    return adj[indexAdj++];
                }
                else
                {
                    indexAdj = 0;
                    return null;
                }
            }

            public Edge getEdgeToV(VertexAlgo VA)
            {
                foreach (Edge e in v.edges_out)
                {
                    if (e.v_to == VA.getV())
                    {
                        e.v_to.around = EdgeFocus;
                        e.color = EdgeFocus;
                        if (e.TwoSided)
                        {
                            foreach (Edge et in graph.E)
                            {
                                if (et.v_from == e.v_to && et.v_to == this.getV())
                                {
                                    et.color = EdgeFocus;
                                }
                            }

                        }
                        return e;
                    }
                }
                return null;
            }
            public Edge getEdgeToV(Vertex V)
            {
                foreach (Edge e in v.edges_out)
                {
                    if (e.v_to == V)
                    {
                        e.v_to.around = EdgeFocus;
                        e.color = EdgeFocus;
                        if (e.TwoSided)
                        {
                            foreach (Edge et in graph.E)
                            {
                                if(et.v_from == e.v_to && et.v_to == this.getV())
                                {
                                    et.color = EdgeFocus;
                                }
                            }
                            
                        }
                        return e;
                    }
                }
                return null;
            }
            public void focus(bool Focus, bool Exclusive = true)
            {
                if (Exclusive)
                    graph.RemoveFocus();
                v.Focus = Focus;
                v.around = ColorFocus;
                refresh();
            }

            public void clearColorEdgeFromAdj()
            {
                foreach (Edge e in v.edges_out)
                {
                    e.color = e.color != EdgeFinished ? EdgeDefault : EdgeFinished;
                }
                refresh();
            }
            public void focusOnEdgesFromAdj(int j = -1)
            {
                if (!(j >= 0 && j < v.edges_out.Count))
                    return;
                foreach (Edge e in v.edges_out)
                {
                    e.color = EdgeFocus;
                    v.edges_out[j].v_to.Focus = false;
                    v.edges_out[j].v_to.around = ColorFocus;
                }
                if (j >= 0 && j < v.edges_out.Count)
                {
                    v.edges_out[j].v_to.Focus = true;
                }
                refresh();
            }

            public static void ClearFocus()
            {
                foreach (Vertex v in graph.V)
                {
                    v.Focus = false;
                    v.around = ColorFocus;
                    foreach (Edge e in v.edges_out)
                    {
                        if (v.Color == ColorFinished)
                        {
                            e.color = EdgeFinished;
                            if (e.TwoSided) { 
                                foreach (Edge et in graph.E)
                                {
                                    if(et.v_from == e.v_to && et.v_to == e.v_from)
                                        et.color = EdgeFinished;
                                }
                            }
                        }
                        else
                        {
                            e.color = e.color != EdgeFinished ? EdgeDefault : e.color;
                            if (e.TwoSided)
                            {
                                foreach (Edge et in graph.E)
                                {
                                    if (et.v_from == e.v_to && et.v_to == e.v_from)
                                        et.color = e.color != EdgeFinished ? EdgeDefault : e.color;
                                }
                            }
                        }
                    }
                }

                refresh();
            }
            private static void refresh()
            {
                graph.main_panel.Refresh();
            }
        }
        private bool[,] matTree_checked;

        

        /* Get the tree graph of the result */
        private void initTreeGraph()
        {
            btn_switch_graph.Visible = true;
            matTree_checked = new bool[V_Chars.Count+1, V_Chars.Count+1];
            for (int i = 0; i < matTree_checked.GetLength(1); i++)
            {
                for (int j = 0; j < matTree_checked.GetLength(0); j++)
                {
                    matTree_checked[i, j] = false;
                }
            }

            for (int p = 1; p < mat_result.GetLength(1); p++)
            {
                if(!(mat_result[2, p].Text.Equals("NIL")))
                {
                    int pi_u = (new VertexAlgo(mat_result[2, p].Text).indexV);
                    int u = (new VertexAlgo(mat_result[0, p].Text).indexV);
                    matTree_checked[pi_u, u] = true;
                }
            }

            graphTree = new Graph(ref panel_tree_panel, V_Chars, matTree_checked, true, (int)SIZE_V.NORMAL, true);
            showTreeGraph = false;
            graphTree.VetexCotrols(false);
        }
        private bool showTreeGraph { get; set; }
        private void btn_switch_graph_Click(object sender, EventArgs e)
        {
            showTreeGraph = !showTreeGraph;
            btn_switch_graph.ButtonText = showTreeGraph ? "גרף קודקודים" : "עץ המרחקים הקצרים";
            if (showTreeGraph)
            {
                panel_tree_panel.Visible = true;
                panel_tree_panel.BringToFront();
                graphTree.VetexCotrols(true);
            }
            else
            {
                panel_tree_panel.Visible = false;
                panel_tree_panel.SendToBack();
                graphTree.VetexCotrols(false);
            }
        }

        /* set resize panels by drop drag lable hr */
        /* Handler the Drop & Down on labels */
        bool allowResize = false;
        private void pictureBox_hr_MouseUp(object sender, MouseEventArgs e)
        {
            allowResize = false;
            PictureBox p = sender as PictureBox;
            if (p == pictureBox_hr_output_graph)
            {
                p.Image = up_down;
            }
            else if (p == pictureBox_hr_algo_graph)
            {
                p.Image = right_left;
            }
            graph.insertVtoFrame();
        }
        private void pictureBox_hr_MouseDown(object sender, MouseEventArgs e)
        {
            allowResize = true;
            PictureBox p = sender as PictureBox;
            if (p == pictureBox_hr_output_graph)
            {
                p.Image = up_down_full;
            }
            else if (p == pictureBox_hr_algo_graph)
            {
                p.Image = right_left_full;
            }
        }
        private void pictureBox_hr_MouseMove(object sender, MouseEventArgs e)
        {
            if (allowResize)
            {
                PictureBox p = sender as PictureBox;
                if (p == pictureBox_hr_output_graph) {
                    int h = this.Height - panel_output.Top - e.Y;
                    if (h > header.Top + header.Height && h < this.Height - header.Height)
                    {
                        this.panel_output.Height = h;
                        this.panel_tree_panel.Height = this.Height - this.panel_output.Top;
                    }
                }
                else if (p == pictureBox_hr_algo_graph) {
                    int w = pictureBox_hr_algo_graph.Left + pictureBox_hr_algo_graph.Width + e.X;
                    if (w >= 390 && w < this.Width - panel_slidemenu.Width)
                    {
                        this.panel_algo.Width = w;
                    }
                }
            }
        }
        private void setResizePanels()
        {
            pictureBox_hr_output_graph.MouseDown += new MouseEventHandler(pictureBox_hr_MouseDown);
            pictureBox_hr_output_graph.MouseMove += new MouseEventHandler(pictureBox_hr_MouseMove);
            pictureBox_hr_output_graph.MouseUp += new MouseEventHandler(pictureBox_hr_MouseUp);

            pictureBox_hr_algo_graph.MouseDown += new MouseEventHandler(pictureBox_hr_MouseDown);
            pictureBox_hr_algo_graph.MouseMove += new MouseEventHandler(pictureBox_hr_MouseMove);
            pictureBox_hr_algo_graph.MouseUp += new MouseEventHandler(pictureBox_hr_MouseUp);
        }
    }
}
       
