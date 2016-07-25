using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace 踩地雷
{   
    /*https://kelunyang.wordpress.com/2008/04/22/c%E7%9A%84%E8%B8%A9%E5%9C%B0%E9%9B%B7%E9%81%8A%E6%88%B2%EF%BC%88console%E7%89%88%EF%BC%89/ */
   
        /*這個程式是模仿Windows踩地雷寫的，地雷數和棋盤數都是參照Windows踩地雷的規格，因此有名次存檔，而遊戲本身並不需要存檔
    
   *基本原則為：

   *1.準備三個陣列，一個是真正放置答案的陣列、一個是將玩家選擇的結果存起來的陣列，另外一個是負責顯示的陣列

   *2.在主程式區跑兩個迴圈，第一個迴圈只負責定義陣列和一些數值，另外一個迴圈負責和使用者互動，結束遊戲的方法則是利用break脫離兩個迴圈

   *虛擬碼為：

   *1.呼叫FileOp.checkFile函式，開啟存檔名次

   *2.呼叫selecttable函式進入歡迎選單，out bombmax(炸彈數目);out horizon(高度);out width(寬度)

   *3.呼叫bombCreate和hintCreate函式，建立地圖tableArr陣列

   *4.執行cheat函式，先預覽一下地雷在哪裡

   *5.呼叫initialMirror函式，Clone一份tableArr陣列

   *6.ConsoleKey cki開始讀鍵值

   *7.上下左右就是移動方位x+1,x-1,y+1,y-1

   *8.按下Enter時tableArrtemp[y,x] = 17 插旗幟

   *9.按下Space時翻開空格，tableArrtemp[y,x] = tableArr[y,x]

   *10.迴圈結束，執行zerofinder函式，掃描陣列，要是遇到0就自動翻開八格

   *11.執行evaluate函式，掃描tableArrtemp[y,x]=17的點是否等於tableArr[y,x]=9(地雷)，相符的話bombmax–，當bombmax歸零時，遊戲結束

   */
    class Program
    {
        const char filledblock = '\u25A0';//未翻開的格子
        const char emptyblock = '\u25A1'; //空格子
        const char bomb = '\u25CF';// 炸彈
        const char flag = '\u22BF'; //旗標
        const char selected = '\u2573'; //目前位置
        static void Main(string[] args)
        {
            Console.SetWindowSize(65, 35); //改變視窗大小否則30*16的地圖放不下
            bool gamecontinued = true;           
            while (gamecontinued)
            {
                string[] recname = new string[10];
                long[] rectime   = new long[10];
                FileOp.checkfill(ref recname, ref rectime);
                int bombmax, width, horizon;
                selectTable(out bombmax, out width, out horizon, out gamecontinued, recname, rectime);

                if (!gamecontinued) break;
                Console.Clear();
                int[,] tableArr = new int[horizon, width];

                bombCreate(tableArr, bombmax);  hintCreate(tableArr);

                int[,] tableArrMirror =         new int[horizon, width];
                int[,] tableArrMirrortemp =     new int[horizon, width];
                int[,] cheatArr =               new int[horizon, width]; //這個是作弊用
                int selectiony = 0;             int selectionx = 0;
                int temppositionx = 0;          int temppositiony = 0;
                bool loss = false;              bool escape;
                int guessetimes = bombmax;      int guessed = 0;
                bool toggle = true;             long current = DateTime.Now.Ticks;
                Console.WriteLine("準備中，請稍後...");
                cheatArr(cheatArr, tableArr, current, guessetimes);
                initialMirror(tableArrMirror, tableArrMirrortemp);
                drawTable(tableArrMirror, guessetimes, true, bombmax, toggle);
                ConsoleKeyInfo cki;
                do
                {
                    toggle = true;
                    cki = Console.ReadKey(true);
                    if (
                        cki.Key == ConsoleKey.UpArrow    ||  cki.Key == ConsoleKey.DownArrow  ||
                        cki.Key == ConsoleKey.LeftArrow  ||  cki.Key == ConsoleKey.RightArrow ||
                        cki.Key == ConsoleKey.NumPad8    ||  cki.Key == ConsoleKey.NumPad2    ||
                        cki.Key == ConsoleKey.NumPad4    ||  cki.Key == ConsoleKey.NumPad6
                        )
                    {
                        if (cki.Key == ConsoleKey.UpArrow || cki.Key == ConsoleKey.NumPad8)
                        {
                            temppositiony = selectiony;  temppositionx = selectionx;
                            try
                            {
                                selectiony -= 1;
                                tableArrMirror[selectiony, selectionx] = 16;
                                tableArrMirror[temppositiony, temppositionx] = tableArrMirrortemp[temppositiony, temppositionx];
                            }
                            catch
                            {
                                selectiony = horizon - 1;
                                tableArrMirror[selectiony, selectionx] = 16;
                                tableArrMirror[temppositiony, temppositionx] = tableArrMirrortemp[temppositiony, temppositionx];
                            }
                        }
                        else if (cki.Key == ConsoleKey.DownArrow || cki.Key == ConsoleKey.NumPad2)
                        {
                            temppositiony = selectiony;  temppositionx = selectionx;
                            try
                            {
                                selectiony += 1;
                                tableArrMirror[selectiony, selectionx] = 16;
                                tableArrMirror[temppositiony, temppositionx] = tableArrMirrortemp[temppositiony, temppositionx];
                            }
                            catch
                            {
                                selectionx = 0;
                                tableArrMirror[selectiony, selectionx] = 16;
                                tableArrMirror[temppositiony, temppositionx] = tableArrMirrortemp[temppositiony, temppositionx];
                            }
                        }
                        else if (cki.Key == ConsoleKey.RightArrow || cki.Key == ConsoleKey.NumPad6)
                        {
                            temppositiony = selectiony;  temppositionx = selectionx;
                            try
                            {
                                selectionx++;
                                tableArrMirror[selectiony, selectionx] = 16;
                                tableArrMirror[temppositiony, temppositionx] = tableArrMirrortemp[temppositiony, temppositionx];
                            }
                            catch
                            {
                                selectionx = 0;
                                tableArrMirror[selectiony, selectionx] = 16;
                                tableArrMirror[temppositiony, temppositionx] = tableArrMirrortemp[temppositiony, temppositionx];
                            }
                        }
                        else if (cki.Key == ConsoleKey.LeftArrow || cki.Key == ConsoleKey.NumPad4)
                        {
                            temppositiony = selectiony;  temppositionx = selectionx;
                            try
                            {
                                selectionx -- ;
                                tableArrMirror[selectiony, selectionx] = 16;
                                tableArrMirror[temppositiony, temppositionx] = tableArrMirrortemp[temppositiony, temppositionx];
                            }
                            catch
                            {
                                selectionx = width - 1;
                                tableArrMirror[selectiony, selectionx] = 16;
                                tableArrMirror[temppositiony, temppositionx] = tableArrMirrortemp[temppositiony, temppositionx];
                            }
                        }
                    }
                    if ((cki.Key == ConsoleKey.Spacebar || cki.Key == ConsoleKey.NumPad0) && tableArrMirrortemp[selectiony, selectionx] == 15 )
                    {
                        tableArrMirrortemp[selectiony, selectionx] = tableArr[selectiony, selectionx];
                        tableArrMirror[selectiony, selectionx]     = tableArrMirrortemp[selectiony, selectionx];
                        if(tableArr[selectiony, selectionx] == 0)
                        {
                            tableArrMirrortemp[selectiony, selectionx] = tableArr[selectiony, selectionx];
                            tableArrMirror[selectiony, selectionx]     = tableArr[selectiony, selectionx];
                            zerofinder(tableArr, tableArrMirror, tableArrMirrortemp);
                        }
                        if(tableArr[selectiony, selectionx] == 9) { loss = true; }
                    }
                    if(cki.Key == ConsoleKey.Enter)
                    {
                        if(guessetimes != 0 && tableArrMirrortemp[selectiony, selectionx] == 15)
                        {
                            guessetimes--;
                            tableArrMirrortemp[selectiony, selectionx] = 17;
                            tableArrMirror[selectiony, selectionx] = 17;
                        }
                        else if (tableArrMirrortemp[selectiony, selectionx] == 17)
                        {
                            guessetimes++;
                            tableArrMirrortemp[selectiony, selectionx] = 15;
                            tableArrMirror[selectiony, selectionx] = 15;
                        }
                    }
                    if(cki.Key == ConsoleKey.K)
                    {
                        Console.WriteLine("作弊鍵啟動，直接獲勝~");
                        for (int y = tableArr.GetLowerBound(0); y <= tableArr.GetUpperBound(0); y++) 
                    }
                }
            }
        }
    }
}
