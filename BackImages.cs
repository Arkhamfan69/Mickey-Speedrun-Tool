// using System;
// using System.Drawing;
// using System.IO;
// using System.Windows.Forms;

// namespace MickeySpeedrunTool.UI
// {
//     internal static class BackImages
//     {
//         private static readonly Random Random = new();

//         public static void ApplyRandomBackground(Form form)
//         {
//             try
//             {
//                 string basePath = AppContext.BaseDirectory;
//                 string backgroundsFolder = Path.Combine(basePath, "Important Stuff", "Assets", "Backgrounds");

//                 if (!Directory.Exists(backgroundsFolder))
//                 {
//                     return;
//                 }

//                 string[] validExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
//                 var files = Directory.GetFiles(backgroundsFolder);
//                 var imageFiles = Array.FindAll(files, file => Array.Exists(validExtensions, ext => ext.Equals(Path.GetExtension(file), StringComparison.OrdinalIgnoreCase)));

//                 if (imageFiles.Length == 0)
//                 {
//                     return;
//                 }

//                 string selectedFile = imageFiles[Random.Next(imageFiles.Length)];

//                 using Bitmap temp = new(selectedFile);

//                 form.BackgroundImage = new Bitmap(temp);
//                 form.BackgroundImageLayout = ImageLayout.Stretch;
//             }
//             catch (Exception ex)
//             {
//                 MessageBox.Show(
//                     $"Failed to load background.\n\n{ex.Message}",
//                     "Background Error",
//                     MessageBoxButtons.OK,
//                     MessageBoxIcon.Error
//                 );
//             }
//         }
//     }
// } Maybe for v2.0, but for now, this is unused and can be removed.