using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace AreaParty.util
{
    class IconUtil
    {
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        private struct POINT
        {
            public int x;
            public int y;
        }
        // Constants that we need in the function call

        private const int SHGFI_ICON = 0x100;

        private const int SHGFI_SMALLICON = 0x1;

        private const int SHGFI_LARGEICON = 0x0;

        private const int SHIL_JUMBO = 0x4;
        private const int SHIL_EXTRALARGE = 0x2;

        // This structure will contain information about the file

        public struct SHFILEINFO
        {

            // Handle to the icon representing the file

            public IntPtr hIcon;

            // Index of the icon within the image list

            public int iIcon;

            // Various attributes of the file

            public uint dwAttributes;

            // Path to the file

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]

            public string szDisplayName;

            // File type

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]

            public string szTypeName;

        };

        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        public static extern Boolean CloseHandle(IntPtr handle);

        private struct IMAGELISTDRAWPARAMS
        {
            public int cbSize;
            public IntPtr himl;
            public int i;
            public IntPtr hdcDst;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int xBitmap;        // x offest from the upperleft of bitmap
            public int yBitmap;        // y offset from the upperleft of bitmap
            public int rgbBk;
            public int rgbFg;
            public int fStyle;
            public int dwRop;
            public int fState;
            public int Frame;
            public int crEffect;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IMAGEINFO
        {
            public IntPtr hbmImage;
            public IntPtr hbmMask;
            public int Unused1;
            public int Unused2;
            public Rect rcImage;
        }

        #region Private ImageList COM Interop (XP)
        [ComImportAttribute()]
        [GuidAttribute("46EB5926-582E-4017-9FDF-E8998DAA0950")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        //helpstring("Image List"),
        interface IImageList
        {
            [PreserveSig]
            int Add(
                IntPtr hbmImage,
                IntPtr hbmMask,
                ref int pi);

            [PreserveSig]
            int ReplaceIcon(
                int i,
                IntPtr hicon,
                ref int pi);

            [PreserveSig]
            int SetOverlayImage(
                int iImage,
                int iOverlay);

            [PreserveSig]
            int Replace(
                int i,
                IntPtr hbmImage,
                IntPtr hbmMask);

            [PreserveSig]
            int AddMasked(
                IntPtr hbmImage,
                int crMask,
                ref int pi);

            [PreserveSig]
            int Draw(
                ref IMAGELISTDRAWPARAMS pimldp);

            [PreserveSig]
            int Remove(
            int i);

            [PreserveSig]
            int GetIcon(
                int i,
                int flags,
                ref IntPtr picon);

            [PreserveSig]
            int GetImageInfo(
                int i,
                ref IMAGEINFO pImageInfo);

            [PreserveSig]
            int Copy(
                int iDst,
                IImageList punkSrc,
                int iSrc,
                int uFlags);

            [PreserveSig]
            int Merge(
                int i1,
                IImageList punk2,
                int i2,
                int dx,
                int dy,
                ref Guid riid,
                ref IntPtr ppv);

            [PreserveSig]
            int Clone(
                ref Guid riid,
                ref IntPtr ppv);

            [PreserveSig]
            int GetImageRect(
                int i,
                ref RECT prc);

            [PreserveSig]
            int GetIconSize(
                ref int cx,
                ref int cy);

            [PreserveSig]
            int SetIconSize(
                int cx,
                int cy);

            [PreserveSig]
            int GetImageCount(
            ref int pi);

            [PreserveSig]
            int SetImageCount(
                int uNewCount);

            [PreserveSig]
            int SetBkColor(
                int clrBk,
                ref int pclr);

            [PreserveSig]
            int GetBkColor(
                ref int pclr);

            [PreserveSig]
            int BeginDrag(
                int iTrack,
                int dxHotspot,
                int dyHotspot);

            [PreserveSig]
            int EndDrag();

            [PreserveSig]
            int DragEnter(
                IntPtr hwndLock,
                int x,
                int y);

            [PreserveSig]
            int DragLeave(
                IntPtr hwndLock);

            [PreserveSig]
            int DragMove(
                int x,
                int y);

            [PreserveSig]
            int SetDragCursorImage(
                ref IImageList punk,
                int iDrag,
                int dxHotspot,
                int dyHotspot);

            [PreserveSig]
            int DragShowNolock(
                int fShow);

            [PreserveSig]
            int GetDragImage(
                ref POINT ppt,
                ref POINT pptHotspot,
                ref Guid riid,
                ref IntPtr ppv);

            [PreserveSig]
            int GetItemFlags(
                int i,
                ref int dwFlags);

            [PreserveSig]
            int GetOverlayImage(
                int iOverlay,
                ref int piIndex);
        };
        #endregion

        ///
        /// SHGetImageList is not exported correctly in XP.  See KB316931
        /// http://support.microsoft.com/default.aspx?scid=kb;EN-US;Q316931
        /// Apparently (and hopefully) ordinal 727 isn't going to change.
        ///
        [DllImport("shell32.dll", EntryPoint = "#727")]
        private extern static int SHGetImageList(
            int iImageList,
            ref Guid riid,
            out IImageList ppv
            );

        // The signature of SHGetFileInfo (located in Shell32.dll)
        [DllImport("Shell32.dll")]
        public static extern int SHGetFileInfo(string pszPath, int dwFileAttributes, ref SHFILEINFO psfi, int cbFileInfo, uint uFlags);
        

        
        /// <summary>
        /// 获取文件图标
        /// </summary>
        /// <param name="FileName">文件路径</param>
        /// <param name="small">是否为小图标，true为16*16，false为32*32</param>
        /// <param name="checkDisk"></param>
        /// <param name="addOverlay">是否包含连接</param>
        /// <returns></returns>
        public static Icon Icon_Of_Path(string FileName, bool small, bool checkDisk, bool addOverlay)
        {
            if (!File.Exists(FileName)) return null;
            SHFILEINFO shinfo = new SHFILEINFO();

            uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
            uint SHGFI_LINKOVERLAY = 0x000008000;

            uint flags;
            if (small)
            {
                flags = SHGFI_ICON | SHGFI_SMALLICON;
            }
            else
            {
                flags = SHGFI_ICON | SHGFI_LARGEICON;
            }
            if (!checkDisk)
                flags |= SHGFI_USEFILEATTRIBUTES;
            if (addOverlay)
                flags |= SHGFI_LINKOVERLAY;

            var res = SHGetFileInfo(FileName, 0, ref shinfo, Marshal.SizeOf(shinfo), flags);
            if (res == 0)
            {
                throw (new System.IO.FileNotFoundException());
            }

            var myIcon = System.Drawing.Icon.FromHandle(shinfo.hIcon);
            
            return myIcon;

        }
        public static void IconSave(Icon icon ,string filename)
        {
            if (icon == null) return;
            FileStream fileStream = new FileStream(filename, FileMode.Create);
            icon.ToBitmap().Save(fileStream, ImageFormat.Png);
            fileStream.Close();
        }
        /// <summary>
        /// 获取指定文件图标的指定大小，并保存在指定路径中。
        /// </summary>
        /// <param name="fileName">文件路径</param>
        /// <param name="size">图标大小：0为16*16，1为32*32，2为48*48，3为256*256</param>
        /// <param name="iconFileName">保存图标路径</param>
        public static void GetIconFromFile(string fileName,int size,string iconFileName)
        {
            Icon icon = null;
            switch(size){
                case 0:
                    icon=Icon_Of_Path(fileName, true, true, false);
                    IconSave(icon, iconFileName);
                    break;
                case 1:
                    icon = Icon_Of_Path(fileName, false, true, false);
                    IconSave(icon, iconFileName);
                    break;
                case 2:
                    icon = Icon_Of_Path_Large(fileName, false, true);
                    IconSave(icon, iconFileName);
                    break;
                case 3:
                    icon = Icon_Of_Path_Large(fileName, true, true);
                    IconSave(icon, iconFileName);
                    break;
            }
        }
        /// <summary>
        /// 获取文件图标
        /// 
        /// </summary>
        /// <param name="FileName">文件路径</param>
        /// <param name="jumbo">是否为超大图标，true为256*256,false为48*48</param>
        /// <param name="checkDisk"></param>
        /// <returns></returns>
        public static Icon Icon_Of_Path_Large(string FileName, bool jumbo, bool checkDisk)
        {
            if (!File.Exists(FileName)) return null;
            SHFILEINFO shinfo = new SHFILEINFO();

            uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
            uint SHGFI_SYSICONINDEX = 0x4000;

            int FILE_ATTRIBUTE_NORMAL = 0x80;

            uint flags;
            flags = SHGFI_SYSICONINDEX;

            if (!checkDisk)  // This does not seem to work. If I try it, a folder icon is always returned.
                flags |= SHGFI_USEFILEATTRIBUTES;

            var res = SHGetFileInfo(FileName, FILE_ATTRIBUTE_NORMAL, ref shinfo, Marshal.SizeOf(shinfo), flags);
            if (res == 0)
            {
                throw (new System.IO.FileNotFoundException());
            }
            var iconIndex = shinfo.iIcon;

            // Get the System IImageList object from the Shell:
            Guid iidImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");

            IImageList iml;
            int size = jumbo ? SHIL_JUMBO : SHIL_EXTRALARGE;
            var hres = SHGetImageList(size, ref iidImageList, out iml); // writes iml
                                                                        //if (hres == 0)
                                                                        //{
                                                                        //    throw (new System.Exception("Error SHGetImageList"));
                                                                        //}
            IntPtr hIcon = IntPtr.Zero;
            int ILD_TRANSPARENT = 1;
            hres = iml.GetIcon(iconIndex, ILD_TRANSPARENT, ref hIcon);
            //if (hres == 0)
            //{
            //    throw (new System.Exception("Error iml.GetIcon"));
            //}

            var myIcon = System.Drawing.Icon.FromHandle(hIcon);

            return myIcon;

        }
    }
}
