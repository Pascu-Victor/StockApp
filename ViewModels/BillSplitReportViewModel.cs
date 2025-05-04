﻿using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Src.Model;
using StockApp.Services;

namespace StockApp.ViewModels
{
    public class BillSplitReportViewModel
    {
        private readonly IBillSplitReportService billSplitReportService;

        public ObservableCollection<BillSplitReport> BillSplitReports { get; set; }

        public BillSplitReportViewModel()
        {
            BillSplitReports = new ObservableCollection<BillSplitReport>(billSplitReportService.GetBillSplitReports());
        }

        public async Task LoadBillSplitReports()
        {
            try
            {
                var reports = billSplitReportService.GetBillSplitReports();
                foreach (var report in reports)
                {
                    BillSplitReports.Add(report);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error: {exception.Message}");
            }
        }
    }
}
