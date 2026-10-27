<p align="center">
  <img src="logo_yoto.png" width="120" alt="YOTO-IA">
</p>

<h1 align="center">Pantalla Prendida</h1>
<p align="center"><b>by YOTO-IA</b></p>

Una pequeña ventana para Windows que se queda en la esquina superior derecha de la pantalla y, con un solo interruptor, evita que tu PC o laptop se apague o se suspenda.

## ¿Qué hace?

¿Te ha pasado que estás descargando algo, viendo una clase, dejando un programa trabajando o mostrando algo en pantalla, y de repente la **pantalla se apaga** o la **laptop se suspende**? Pantalla Prendida lo evita.

Solo enciendes el interruptor y tu computadora **se queda prendida todo el tiempo que quieras**, incluso si **cierras la tapa de la laptop**. Cuando lo apagas, todo vuelve a la normalidad, sin cambiar para siempre tu configuración de energía.

Es ideal para:

- Descargas largas que no deben cortarse.
- Clases, reuniones o videos sin que la pantalla se apague.
- Dejar programas trabajando (renders, copias de seguridad, servidores, bots).
- Usar la laptop con la tapa cerrada conectada a un monitor.

## Funciones

- **ON**: la pantalla no se apaga y la PC no entra en suspensión.
- **Tapa de la laptop**: en ON, cerrar la tapa no suspende la laptop. Al pasar a OFF o cerrar la app, se restaura la configuración original.
- **OFF**: la PC funciona normal, con tu configuración de energía de siempre.
- **Abrir al prender la PC**: opción para que la app arranque sola con Windows.
- Siempre visible, se puede arrastrar, minimizar y cerrar.
- No necesita instalación ni permisos de administrador.

## Descargar

Descarga [**PantallaPrendida.exe**](../../raw/main/PantallaPrendida.exe) y ábrelo con doble clic. No necesita instalación.

> Si Windows muestra "Windows protegió su PC", haz clic en **Más información → Ejecutar de todas formas**. Aparece porque el programa no está firmado digitalmente.

## Compilar desde el código

No necesita Visual Studio; usa el compilador de C# que ya viene con Windows:

```powershell
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /nologo /target:winexe /codepage:65001 /win32icon:icono.ico /resource:logo_yoto.png,logo.png /out:PantallaPrendida.exe /r:System.Windows.Forms.dll /r:System.Drawing.dll VentanaPrendida.cs
```

## Autor

© 2026 **YOTO-IA**
