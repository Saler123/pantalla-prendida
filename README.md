<p align="center">
  <img src="logo_yoto.png" width="120" alt="YOTO-IA">
</p>

<h1 align="center">Pantalla Prendida</h1>
<p align="center"><b>by YOTO-IA</b></p>

Una pequeña ventana para Windows que se queda en la esquina superior derecha de la pantalla y, con un solo interruptor, evita que tu PC o laptop se apague o se suspenda.

## Funciones

- **ON**: la pantalla no se apaga y la PC no entra en suspensión.
- **Tapa de la laptop**: en ON, cerrar la tapa no suspende la laptop. Al pasar a OFF o cerrar la app, se restaura la configuración original.
- **OFF**: la PC funciona normal, con tu configuración de energía de siempre.
- **Abrir al prender la PC**: opción para que la app arranque sola con Windows.
- Siempre visible, se puede arrastrar, minimizar y cerrar.
- No necesita instalación ni permisos de administrador.

## Descargar

Descarga `PantallaPrendida.exe` desde la sección [**Releases**](../../releases) y ábrelo con doble clic.

> Si Windows muestra "Windows protegió su PC", haz clic en **Más información → Ejecutar de todas formas**. Aparece porque el programa no está firmado digitalmente.

## Compilar desde el código

No necesita Visual Studio; usa el compilador de C# que ya viene con Windows:

```powershell
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /nologo /target:winexe /codepage:65001 /win32icon:icono.ico /resource:logo_yoto.png,logo.png /out:PantallaPrendida.exe /r:System.Windows.Forms.dll /r:System.Drawing.dll VentanaPrendida.cs
```

## Autor

© 2026 **YOTO-IA**
