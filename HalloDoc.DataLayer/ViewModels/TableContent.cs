using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.DataLayer.ViewModels;

public class TableContent
{
    public int RequestId { get; set; }
    public DateTime CreatedDate { get; set; }
    public short Status { get; set; }
    public string Name { get; set; }
    public long? Count { get; set; }
    public string FileName { get; set; }
}