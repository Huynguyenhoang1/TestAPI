using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using MoreLinq;
using Nice3point.Revit.Toolkit.External;


namespace TestTool.Commands
{
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class Command : ExternalCommand
    {
        public override void Execute()
        {
            Transaction transaction = new Transaction(Document, "Charge Type System");
            transaction.Start();


            var listConector = new FilteredElementCollector(Document, Document.ActiveView.Id)
                .WhereElementIsNotElementType().ToElements().Where(x => x is Pipe).ToList()
                .Select(x => GetConnectors(x)).Flatten().Select(x => x as Connector).ToList();

            // Lấy đối tượng Connector của ống đầu tiên
            var connectors1 = GetConnectors((Document.GetElement(UiDocument.Selection.PickObject(ObjectType.Element)) as Pipe));

            connectors1.ForEach(x =>
            {
                listConector.ForEach(a =>
                {
                    if (x.IsConnectedTo(a))
                    {
                        x.DisconnectFrom(a);
                    }
                });
            });



            transaction.Commit();
        }

        public  List<Connector> GetConnectors( Element element)
        {
            List<Connector> result = new List<Connector>();
            try
            {
                try
                {
                    result = (from Connector x in (element as MEPCurve).ConnectorManager.Connectors
                        orderby x.Origin.Z
                        select x).ToList<Connector>();
                }
                catch
                {
                    result = (from Connector x in (element as FamilyInstance).MEPModel.ConnectorManager.Connectors
                        orderby x.Origin.Z
                        select x).ToList<Connector>();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }
   }
}