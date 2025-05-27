using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public enum DistanceMetric { Euclidean, Manhattan }

public class VoronoiForm : Form
{
    private List<Point> vertices = new List<Point>();
    private Bitmap voronoiImage;
    private bool isMultithreaded = false;
    private DistanceMetric currentMetric = DistanceMetric.Euclidean;
    private Random rand = new Random();
    private const int regionCount = 4;

    public VoronoiForm()
    {
        this.DoubleBuffered = true;
        this.WindowState = FormWindowState.Maximized;
        this.MouseClick += OnMouseClick;

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Toggle Threads", null, (s, e) => { isMultithreaded = !isMultithreaded; Recompute(); });
        contextMenu.Items.Add("Clear Vertices", null, (s, e) => { vertices.Clear(); Recompute(); });
        contextMenu.Items.Add("Add Random Vertices", null, (s, e) => { AddRandomVertices(50); });
        contextMenu.Items.Add("Switch Metric", null, (s, e) => { SwitchMetric(); });
        this.ContextMenuStrip = contextMenu;

        Recompute();
    }

    private void OnMouseClick(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            vertices.Add(e.Location);
        else if (e.Button == MouseButtons.Right && vertices.Count > 0)
            vertices.RemoveAt(vertices.Count - 1);

        Recompute();
    }

    private void AddRandomVertices(int count)
    {
        for (int i = 0; i < count; i++)
            vertices.Add(new Point(rand.Next(ClientSize.Width), rand.Next(ClientSize.Height)));
        Recompute();
    }

    private void SwitchMetric()
    {
        currentMetric = currentMetric == DistanceMetric.Euclidean ? DistanceMetric.Manhattan : DistanceMetric.Euclidean;
        Recompute();
    }

    private void Recompute()
    {
        if (vertices.Count == 0)
        {
            voronoiImage = new Bitmap(ClientSize.Width, ClientSize.Height);
            Invalidate();
            return;
        }

        Stopwatch sw = Stopwatch.StartNew();
        voronoiImage = new Bitmap(ClientSize.Width, ClientSize.Height);

        if (isMultithreaded)
            ComputeVoronoiMultithreaded();
        else
            ComputeVoronoiSingleThreaded();

        sw.Stop();
        Text = $"Voronoi Diagram - Vertices: {vertices.Count} - Mode: {(isMultithreaded ? "Multi" : "Single")} - Metric: {currentMetric} - Time: {sw.ElapsedMilliseconds}ms";
        Invalidate();
    }

    private void ComputeVoronoiSingleThreaded()
    {
        for (int x = 0; x < voronoiImage.Width; x++)
        {
            for (int y = 0; y < voronoiImage.Height; y++)
            {
                int nearest = GetNearestVertexIndex(new Point(x, y));
                voronoiImage.SetPixel(x, y, GetColor(nearest));
            }
        }
    }

    private void ComputeVoronoiMultithreaded()
    {
        int width = voronoiImage.Width;
        int height = voronoiImage.Height;
        int regionWidth = width / regionCount;

        Parallel.For(0, regionCount, regionIndex =>
        {
            int startX = regionIndex * regionWidth;
            int endX = (regionIndex == regionCount - 1) ? width : startX + regionWidth;

            for (int x = startX; x < endX; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int nearest = GetNearestVertexIndex(new Point(x, y));
                    lock (voronoiImage)
                        voronoiImage.SetPixel(x, y, GetColor(nearest));
                }
            }
        });
    }

    private int GetNearestVertexIndex(Point p)
    {
        int minIndex = 0;
        double minDist = GetDistance(p, vertices[0]);

        for (int i = 1; i < vertices.Count; i++)
        {
            double dist = GetDistance(p, vertices[i]);
            if (dist < minDist)
            {
                minDist = dist;
                minIndex = i;
            }
        }
        return minIndex;
    }

    private double GetDistance(Point p1, Point p2)
    {
        switch (currentMetric)
        {
            case DistanceMetric.Euclidean:
                return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
            case DistanceMetric.Manhattan:
                return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
            default:
                return 0;
        }
    }


    private Color GetColor(int index)
    {
        Random r = new Random(index);
        return Color.FromArgb(255, r.Next(256), r.Next(256), r.Next(256));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (voronoiImage != null)
            e.Graphics.DrawImage(voronoiImage, 0, 0);

        foreach (var vertex in vertices)
            e.Graphics.FillEllipse(Brushes.Black, vertex.X - 2, vertex.Y - 2, 5, 5);
    }

    [STAThread]
    public static void Main()
    {
        Application.Run(new VoronoiForm());
    }
}
