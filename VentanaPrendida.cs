using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: AssemblyTitle("Pantalla Prendida")]
[assembly: AssemblyDescription("Mantiene la pantalla prendida y evita la suspensión")]
[assembly: AssemblyCompany("YOTO-IA")]
[assembly: AssemblyProduct("Pantalla Prendida by YOTO-IA")]
[assembly: AssemblyCopyright("© 2026 YOTO-IA")]
[assembly: AssemblyVersion("1.2.0.0")]
[assembly: AssemblyFileVersion("1.2.0.0")]

// Ventana pequeña en la esquina superior derecha que evita que la
// pantalla se apague o el PC entre en suspensión mientras está en ON.
public class VentanaPrendida : Form
{
    [DllImport("kernel32.dll")]
    static extern uint SetThreadExecutionState(uint esFlags);

    const uint ES_CONTINUOUS = 0x80000000;
    const uint ES_SYSTEM_REQUIRED = 0x00000001;
    const uint ES_DISPLAY_REQUIRED = 0x00000002;

    [DllImport("user32.dll")]
    static extern bool ReleaseCapture();
    [DllImport("user32.dll")]
    static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

    // Colores
    static readonly Color Fondo = Color.FromArgb(22, 22, 28);
    static readonly Color Borde = Color.FromArgb(48, 48, 60);
    static readonly Color Texto = Color.FromArgb(240, 240, 245);
    static readonly Color TextoSuave = Color.FromArgb(140, 140, 155);
    static readonly Color Verde = Color.FromArgb(52, 199, 89);
    static readonly Color Apagado = Color.FromArgb(70, 70, 82);
    // Colores del logo YOTO: azul a turquesa
    static readonly Color Marca1 = Color.FromArgb(20, 110, 255);
    static readonly Color Marca2 = Color.FromArgb(0, 225, 200);

    // Logo incrustado dentro del .exe
    readonly Image logo = CargarLogo();

    static Image CargarLogo()
    {
        try
        {
            using (System.IO.Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("logo.png"))
                return s == null ? null : new Bitmap(Image.FromStream(s), 64, 64);
        }
        catch { return null; }
    }

    readonly Font fMarca = new Font("Segoe UI", 7.5f, FontStyle.Bold);
    readonly Font fTitulo = new Font("Segoe UI Semibold", 10f);
    readonly Font fEstado = new Font("Segoe UI", 7.5f);
    readonly Font fPie = new Font("Segoe UI", 7.5f);

    // Zonas clicables
    Rectangle rCerrar, rMinimizar, rSwitch, rAuto;
    string hover = "";

    bool activo = false;
    bool autoInicio;
    float perilla = 0f; // 0 = OFF, 1 = ON (para la animación)
    Timer animacion;
    Timer refresco;

    public VentanaPrendida()
    {
        Text = "Pantalla Prendida";
        FormBorderStyle = FormBorderStyle.None;
        TopMost = true;
        ShowInTaskbar = true;
        StartPosition = FormStartPosition.Manual;
        DoubleBuffered = true;
        Size = new Size(200, 98);
        BackColor = Fondo;
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        Rectangle area = Screen.PrimaryScreen.WorkingArea;
        Location = new Point(area.Right - Width - 12, area.Top + 12);

        rCerrar = new Rectangle(Width - 26, 6, 18, 18);
        rMinimizar = new Rectangle(Width - 48, 6, 18, 18);
        rSwitch = new Rectangle(Width - 14 - 40, 33, 40, 22);
        rAuto = new Rectangle(12, 76, 125, 16);

        using (GraphicsPath forma = Redondeado(new Rectangle(0, 0, Width, Height), 12))
            Region = new Region(forma);

        animacion = new Timer();
        animacion.Interval = 12;
        animacion.Tick += delegate
        {
            float meta = activo ? 1f : 0f;
            perilla += (meta - perilla) * 0.3f;
            if (Math.Abs(meta - perilla) < 0.01f) { perilla = meta; animacion.Stop(); }
            Invalidate();
        };

        // Re-aplica el estado cada 30 s por si Windows lo reinicia
        refresco = new Timer();
        refresco.Interval = 30000;
        refresco.Tick += delegate { if (activo) Aplicar(); };
        refresco.Start();

        autoInicio = LeerAutoInicio();
        ConfigurarAutoInicio(autoInicio);

        Cambiar(false);
    }

    // Sombra alrededor de la ventana
    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            cp.ClassStyle |= 0x20000; // CS_DROPSHADOW
            return cp;
        }
    }

    static GraphicsPath Redondeado(Rectangle r, int radio)
    {
        int d = radio * 2;
        GraphicsPath p = new GraphicsPath();
        p.AddArc(r.X, r.Y, d, d, 180, 90);
        p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }

    static Color Mezclar(Color a, Color b, float t)
    {
        return Color.FromArgb(
            (int)(a.R + (b.R - a.R) * t),
            (int)(a.G + (b.G - a.G) * t),
            (int)(a.B + (b.B - a.B) * t));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        // Borde
        using (GraphicsPath borde = Redondeado(new Rectangle(0, 0, Width - 1, Height - 1), 12))
        using (Pen p = new Pen(Borde))
            g.DrawPath(p, borde);

        // Marca YOTO-IA con degradado
        int xMarca = 10;
        if (logo != null)
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(logo, 10, 6, 16, 16);
            xMarca = 28;
        }
        Rectangle rMarca = new Rectangle(xMarca + 2, 8, 50, 14);
        using (LinearGradientBrush b = new LinearGradientBrush(rMarca, Marca1, Marca2, 0f))
            g.DrawString("YOTO-IA", fMarca, b, xMarca, 7);

        // Botón minimizar
        if (hover == "minimizar")
            using (SolidBrush b = new SolidBrush(Color.FromArgb(60, 60, 72)))
                g.FillEllipse(b, rMinimizar);
        using (Pen p = new Pen(hover == "minimizar" ? Texto : TextoSuave, 1.6f))
            g.DrawLine(p, rMinimizar.X + 5, rMinimizar.Y + 9, rMinimizar.Right - 5, rMinimizar.Y + 9);

        // Botón cerrar
        if (hover == "cerrar")
            using (SolidBrush b = new SolidBrush(Color.FromArgb(196, 43, 28)))
                g.FillEllipse(b, rCerrar);
        using (Pen p = new Pen(hover == "cerrar" ? Texto : TextoSuave, 1.6f))
        {
            int c = 5;
            g.DrawLine(p, rCerrar.X + c, rCerrar.Y + c, rCerrar.Right - c, rCerrar.Bottom - c);
            g.DrawLine(p, rCerrar.Right - c, rCerrar.Y + c, rCerrar.X + c, rCerrar.Bottom - c);
        }

        // Título y estado
        using (SolidBrush b = new SolidBrush(Texto))
            g.DrawString("Pantalla prendida", fTitulo, b, 9, 27);

        Color cEstado = activo ? Verde : TextoSuave;
        using (SolidBrush b = new SolidBrush(cEstado))
        {
            g.FillEllipse(b, 13, 53, 6, 6);
            g.DrawString(activo ? "Activo · no se apagará" : "Modo normal", fEstado, b, 21, 49);
        }

        // Interruptor
        using (GraphicsPath pista = Redondeado(rSwitch, rSwitch.Height / 2))
        using (SolidBrush b = new SolidBrush(Mezclar(Apagado, Verde, perilla)))
            g.FillPath(b, pista);
        int diam = rSwitch.Height - 6;
        float x = rSwitch.X + 3 + (rSwitch.Width - diam - 6) * perilla;
        using (SolidBrush sombra = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
            g.FillEllipse(sombra, x, rSwitch.Y + 4, diam, diam);
        using (SolidBrush b = new SolidBrush(Color.White))
            g.FillEllipse(b, x, rSwitch.Y + 3, diam, diam);

        // Separador
        using (Pen p = new Pen(Borde))
            g.DrawLine(p, 12, 70, Width - 12, 70);

        // Casilla "abrir al prender"
        Rectangle caja = new Rectangle(rAuto.X, rAuto.Y + 2, 11, 11);
        using (GraphicsPath pc = Redondeado(caja, 3))
        {
            if (autoInicio)
            {
                using (SolidBrush b = new SolidBrush(Marca1)) g.FillPath(b, pc);
                using (Pen p = new Pen(Color.White, 1.6f))
                    g.DrawLines(p, new Point[] {
                        new Point(caja.X + 2, caja.Y + 6),
                        new Point(caja.X + 4, caja.Y + 8),
                        new Point(caja.X + 9, caja.Y + 3) });
            }
            else
                using (Pen p = new Pen(hover == "auto" ? Texto : TextoSuave)) g.DrawPath(p, pc);
        }
        using (SolidBrush b = new SolidBrush(hover == "auto" ? Texto : TextoSuave))
            g.DrawString("Abrir al prender la PC", fPie, b, caja.Right + 4, rAuto.Y);

        using (SolidBrush b = new SolidBrush(Color.FromArgb(90, 90, 105)))
            g.DrawString("v1.2", fPie, b, Width - 34, rAuto.Y);
    }

    string ZonaEn(Point pt)
    {
        if (rCerrar.Contains(pt)) return "cerrar";
        if (rMinimizar.Contains(pt)) return "minimizar";
        if (rSwitch.Contains(pt)) return "switch";
        if (rAuto.Contains(pt)) return "auto";
        return "";
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        string z = ZonaEn(e.Location);
        Cursor = z == "" ? Cursors.Default : Cursors.Hand;
        if (z != hover) { hover = z; Invalidate(); }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (hover != "") { hover = ""; Invalidate(); }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left) return;
        string z = ZonaEn(e.Location);
        if (z == "cerrar") Close();
        else if (z == "minimizar") { hover = ""; WindowState = FormWindowState.Minimized; }
        else if (z == "switch") Cambiar(!activo);
        else if (z == "auto")
        {
            autoInicio = !autoInicio;
            ConfigurarAutoInicio(autoInicio);
            Invalidate();
        }
        else
        {
            // Permite arrastrar la ventana desde cualquier parte del fondo
            ReleaseCapture();
            SendMessage(Handle, 0xA1, 0x2, 0); // WM_NCLBUTTONDOWN, HTCAPTION
        }
    }

    const string ClaveRun = @"Software\Microsoft\Windows\CurrentVersion\Run";
    const string ClaveApp = @"Software\PantallaPrendida";

    // Activado por defecto la primera vez; luego respeta lo que elija el usuario
    static bool LeerAutoInicio()
    {
        using (RegistryKey k = Registry.CurrentUser.CreateSubKey(ClaveApp))
            return (int)k.GetValue("AutoInicio", 1) == 1;
    }

    static void ConfigurarAutoInicio(bool on)
    {
        using (RegistryKey k = Registry.CurrentUser.CreateSubKey(ClaveApp))
            k.SetValue("AutoInicio", on ? 1 : 0, RegistryValueKind.DWord);
        using (RegistryKey run = Registry.CurrentUser.CreateSubKey(ClaveRun))
        {
            if (on)
                run.SetValue("PantallaPrendida", "\"" + Application.ExecutablePath + "\"");
            else
                run.DeleteValue("PantallaPrendida", false);
        }
    }

    // --- Tapa de la laptop: al estar en ON, cerrar la tapa no suspende ---

    static string Powercfg(string args)
    {
        System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("powercfg", args);
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;
        psi.RedirectStandardOutput = true;
        using (System.Diagnostics.Process p = System.Diagnostics.Process.Start(psi))
        {
            string salida = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return salida;
        }
    }

    // Devuelve {AC, DC} de la acción al cerrar la tapa, o null si no hay tapa
    static int[] LeerTapa()
    {
        System.Collections.Generic.List<int> valores = new System.Collections.Generic.List<int>();
        foreach (string linea in Powercfg("/qh SCHEME_CURRENT SUB_BUTTONS LIDACTION").Split('\n'))
        {
            int i = linea.IndexOf(": 0x");
            if (i >= 0)
                valores.Add(Convert.ToInt32(linea.Substring(i + 4).Trim(), 16));
        }
        if (valores.Count < 2) return null;
        return new int[] { valores[valores.Count - 2], valores[valores.Count - 1] };
    }

    static void EscribirTapa(int ac, int dc)
    {
        Powercfg("/setacvalueindex SCHEME_CURRENT SUB_BUTTONS LIDACTION " + ac);
        Powercfg("/setdcvalueindex SCHEME_CURRENT SUB_BUTTONS LIDACTION " + dc);
        Powercfg("/setactive SCHEME_CURRENT");
    }

    // Guarda la configuración original en el registro para poder
    // restaurarla aunque la app se cierre de golpe
    static void BloquearTapa()
    {
        using (RegistryKey k = Registry.CurrentUser.CreateSubKey(ClaveApp))
        {
            if (k.GetValue("TapaAC") == null)
            {
                int[] original = LeerTapa();
                if (original == null) return;
                k.SetValue("TapaAC", original[0], RegistryValueKind.DWord);
                k.SetValue("TapaDC", original[1], RegistryValueKind.DWord);
            }
        }
        EscribirTapa(0, 0);
    }

    static void RestaurarTapa()
    {
        using (RegistryKey k = Registry.CurrentUser.CreateSubKey(ClaveApp))
        {
            object ac = k.GetValue("TapaAC");
            object dc = k.GetValue("TapaDC");
            if (ac == null || dc == null) return;
            EscribirTapa((int)ac, (int)dc);
            k.DeleteValue("TapaAC", false);
            k.DeleteValue("TapaDC", false);
        }
    }

    void Cambiar(bool on)
    {
        activo = on;
        if (on) BloquearTapa(); else RestaurarTapa();
        Aplicar();
        animacion.Start();
        Invalidate();
    }

    void Aplicar()
    {
        if (activo)
            SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);
        else
            SetThreadExecutionState(ES_CONTINUOUS);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        SetThreadExecutionState(ES_CONTINUOUS);
        RestaurarTapa();
        base.OnFormClosed(e);
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new VentanaPrendida());
    }
}
