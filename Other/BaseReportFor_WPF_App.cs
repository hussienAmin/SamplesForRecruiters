using KafApp.Models;
using KafApp.Security;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace KafApp.Reports
{
    public abstract class BaseReport
    {
       protected FlowDocument flowDocument ;

        void CreatDocHeader()
        {
            Table table = new Table();
            table.BorderBrush = Brushes.Black;
            table.BorderThickness = new Thickness(0,0,0,1);
            table.CellSpacing = 20;
            table.Columns.Add(new TableColumn() { Width = new GridLength(1, GridUnitType.Auto) });
            table.Columns.Add(new TableColumn() { Width = new GridLength(1, GridUnitType.Star) });
            table.Columns.Add(new TableColumn() { Width = new GridLength(1, GridUnitType.Auto) });
            //table.Columns.Add(new TableColumn() { Width = new GridLength(1, GridUnitType.Star) });

            TableRowGroup group = new TableRowGroup();
           

            TableRow row = new TableRow();
            
            TableCell cell;
            cell = new TableCell(new Paragraph(new Run("dgjhsdgfh")) { Margin = new Thickness(5)});
            row.Cells.Add(cell);
 
            cell = new TableCell(new Paragraph() { Margin = new Thickness(5) });
            row.Cells.Add(cell);
 
            cell = new TableCell(new Paragraph(new Run("gggggggggg")) { Margin = new Thickness(5),TextAlignment=TextAlignment.Right });
            row.Cells.Add(cell);
            group.Rows.Add(row);
            table.RowGroups.Add(group);
            flowDocument.Blocks.Add(table); 

        }
        protected Table CreatTable()
        {
            Table table = new Table();
            table.BorderBrush = Brushes.Black;
            table.BorderThickness = new Thickness(0.5);
            table.CellSpacing = 0; 
            return table;
        }
        protected TableColumn CreatStarColumn()
        =>(new TableColumn() { Width = new GridLength(1, GridUnitType.Star) });

        protected TableColumn CreatAutoColumn()
        =>(new TableColumn() { Width = new GridLength(1, GridUnitType.Auto) });

       
        protected TableCell CreatCell(string txt)
        {
            TableCell cell;

            cell = new TableCell(new Paragraph(new Run(txt)));
            cell.BorderBrush = Brushes.Black;
            cell.BorderThickness = new Thickness(0.5);
          
            return cell;
        }
        protected void CreatTableHeader(Table table, string[] header)
        {
            TableRow row = new TableRow();
            foreach(string s in header)
            {
                TableCell cell;
                cell = new TableCell(new Paragraph(new Run(Helper.GetResource(s))) { Margin = new Thickness(5) });
                cell.BorderBrush = Brushes.Black;
                cell.BorderThickness = new Thickness(0.5);
                cell.FontSize = 16;
                cell.FontWeight= FontWeights.Bold;  
                row.Cells.Add(cell);
            }
           
            
            
            TableRowGroup group = new TableRowGroup();
            group.Rows.Add(row);
            table.RowGroups.Add(group);
        }
        protected TableRow CreatTableRow( string[] header)
        {
            TableRow row = new TableRow();
            foreach(string s in header)
            {
                TableCell cell;
                cell = new TableCell(new Paragraph(new Run(s)) { Margin = new Thickness(5) });
                cell.BorderBrush = Brushes.Black;
                cell.BorderThickness = new Thickness(0.5);
                
                
                row.Cells.Add(cell);
            }

            return row;
           
        }
        public BaseReport()
        {
            flowDocument = new FlowDocument();
            if (Helper.configmodel.Printpagesize == PaperSize.A5)
            {
                flowDocument.ColumnWidth = 350;
                flowDocument.PageWidth = 419.5;
                flowDocument.PageHeight = 595.3;
            }
            else
            {
                flowDocument.ColumnWidth =700;
                flowDocument.PageWidth = 793.7;
                flowDocument.PageHeight = 1122.5;

            }
            flowDocument.Background = Brushes.White;
            flowDocument.Foreground = Brushes.Black;
            flowDocument.PagePadding = new Thickness(20); 
            flowDocument.FontFamily=new FontFamily("Times New Roman"); 
            flowDocument.FontSize = 14;
            CreatDocHeader();
        }
        public abstract FlowDocument GetDoc();
    }
    public class SupplierReport: BaseReport
    {
        public SupplierReport(Supplier md)
        {
            Table table = CreatTable();
            table.Columns.Add(CreatStarColumn());
            table.Columns.Add(CreatStarColumn());
            table.Columns.Add(CreatStarColumn());
            table.Columns.Add(CreatStarColumn());
            table.Columns.Add(CreatStarColumn());
            CreatTableHeader(table, new string[] { "InvoiceNumber","Date","Value","Paid","Notesblock" });



            TableRowGroup group = new TableRowGroup();
            foreach (var item in md.SupplysList)
            {
             group.Rows.Add(CreatTableRow(new string[] {item.Id.ToString(),item.Session.Date,item.NetValue.ToString(),item.Paid.ToString(),item.Notes }));
               
            }
            table.RowGroups.Add(group);


            flowDocument.Blocks.Add(table);

        }
        public override FlowDocument GetDoc()
        {
            return flowDocument;  
        }
    }
}
