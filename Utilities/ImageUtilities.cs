﻿using AhorcadoClient.Views.Dialogs;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AhorcadoClient.Utilities
{
    public static class ImageUtilities
    {
        public static void SetImageSource(Image imageControl, byte[] imageBytes, string defaultImagePath)
        {
            if (IsValidImage(imageBytes))
            {
                using (var ms = new MemoryStream(imageBytes))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    imageControl.Source = image;
                }
            }
            else
            {
                imageControl.Source = LoadBitmapFromPackUri(defaultImagePath);
            }
        }

        public static BitmapImage LoadBitmapFromPackUri(string packUri)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(packUri, UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            return image;
        }

        public static BitmapImage ConvertToImageSource(byte[] imageBytes)
        {
            using (var memoryStream = new MemoryStream(imageBytes))
            {
                return LoadBitmapFromStream(memoryStream);
            }
        }

        public static byte[] ImageToByteArray(BitmapSource bitmapSource)
        {
            if (bitmapSource == null) return null;

            using (var memoryStream = new MemoryStream())
            {
                var encoder = new JpegBitmapEncoder
                {
                    QualityLevel = Constants.DEFAULT_QUALITY
                };

                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static bool AreImagesEqual(byte[] array1, byte[] array2)
        {
            if (array1 == null && array2 == null) return true;
            if (array1 == null || array2 == null) return false;
            if (array1.Length != array2.Length) return false;

            for (int i = 0; i < array1.Length; i++)
                if (array1[i] != array2[i]) return false;

            return true;
        }

        public static bool IsImageSizeValid(byte[] imageBytes)
        {
            return imageBytes != null && imageBytes.Length <= Constants.MAX_IMAGE_SIZE;
        }

        public static BitmapImage LoadAndResizeImage(string path)
        {
            var originalBitmap = LoadBitmapFromFile(path);
            var cropped = CropImageToCenter(originalBitmap);
            var resized = ResizeImage(cropped, Constants.DEFAULT_WIDTH, Constants.DEFAUL_HEIGHT);
            return resized;
        }

        public static BitmapImage LoadBitmapFromFile(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return LoadBitmapFromStream(stream);
            }
        }

        private static BitmapImage LoadBitmapFromStream(Stream stream)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private static CroppedBitmap CropImageToCenter(BitmapImage original)
        {
            double cropSize = Math.Min(original.PixelWidth, original.PixelHeight);
            int x = (original.PixelWidth - (int)cropSize) / 2;
            int y = (original.PixelHeight - (int)cropSize) / 2;

            return new CroppedBitmap(original, new Int32Rect(x, y, (int)cropSize, (int)cropSize));
        }

        private static BitmapImage ResizeImage(BitmapSource source, int width, int height)
        {
            var scaleTransform = new ScaleTransform(
                (double)width / source.PixelWidth,
                (double)height / source.PixelHeight);

            var transformedBitmap = new TransformedBitmap(source, scaleTransform);

            var encoder = new JpegBitmapEncoder
            {
                QualityLevel = Constants.DEFAULT_QUALITY
            };
            encoder.Frames.Add(BitmapFrame.Create(transformedBitmap));

            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return LoadBitmapFromStream(ms);
            }
        }

        public static bool IsValidImage(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return false;

            try
            {
                using (var ms = new MemoryStream(imageBytes))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static byte[] ProcessImageBeforeSaving(string imagePath)
        {
            var resizedBitmap = LoadAndResizeImage(imagePath);
            var compressedImageBytes = ImageToByteArray(resizedBitmap);

            long imageSizeInKb = compressedImageBytes.Length / 1024;

            if (!IsImageSizeValid(compressedImageBytes))
            {
                throw new InvalidOperationException("The processed image exceeds the maximum allowed size.");
            }

            return compressedImageBytes;
        }

        public static byte[] GetProfilePicData(Image imageControl)
        {
            byte[] profilePicData = null;

            if (imageControl?.Source is BitmapImage bitmapImage)
            {
                profilePicData = ImageToByteArray(bitmapImage);

                var defaultImage = new BitmapImage(new Uri(Constants.DEFAULT_PROFILE_PIC_PATH, UriKind.Absolute));
                var defaultImageBytes = ImageToByteArray(defaultImage);

                if (AreImagesEqual(profilePicData, defaultImageBytes))
                {
                    profilePicData = null;
                }
            }

            return profilePicData;
        }

        public static void SelectProfileImage(Image targetImageControl, string dialogTitleResourceKey, Action onImageSet = null)
        {
            var dialogTitle = Application.Current.Resources[dialogTitleResourceKey]?.ToString();

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = Constants.IMAGE_FILE_FILTER,
                Title = dialogTitle
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var processedImageBytes = ProcessImageBeforeSaving(openFileDialog.FileName);

                    if (!IsImageSizeValid(processedImageBytes))
                    {
                        MessageDialog.Show("GlbDialogT_InvalidImageSize", "GlbDialogD_InvalidImageSize", AlertType.WARNING);
                        return;
                    }

                    targetImageControl.Source = ConvertToImageSource(processedImageBytes);
                    onImageSet?.Invoke();
                }
                catch (Exception)
                {
                    MessageDialog.Show("GlbDialogT_InvalidImageSize", "GlbDialogD_InvalidImageSize", AlertType.WARNING);
                }
            }
        }
        public static BitmapImage ByteArrayToImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            using (var ms = new MemoryStream(imageData))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze(); // Para evitar errores de subprocesos
                return image;
            }
        }


    }
}
