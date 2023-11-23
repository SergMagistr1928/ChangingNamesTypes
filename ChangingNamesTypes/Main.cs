using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ChangingNamesTypes
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {

        #region Основоной метод выполняющий все изменения 
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Изменение имён типоразмеров");

                ProcessElementTypes(doc, BuiltInCategory.OST_Windows);
                ProcessElementTypes(doc, BuiltInCategory.OST_Doors);
                ProcessElementTypes(doc, BuiltInCategory.OST_StructuralFraming);
                ProcessElementTypes(doc, BuiltInCategory.OST_StructuralColumns);
                ProcessElementTypes(doc, BuiltInCategory.OST_Floors);
                ProcessElementTypes(doc, BuiltInCategory.OST_Walls);



                tx.Commit();

                TaskDialog.Show("Изменение имён типоразмеров", "Изменение успешно");
            }

            return Result.Succeeded;
        }

        #endregion

        #region  Универсальный Метод выполняющие изменение типоразмеров всех семейств 
        private void ProcessElementTypes(Document doc, BuiltInCategory category)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> elementTypes = collector.OfCategory(category)
                                                          .WhereElementIsElementType()
                                                          .ToElements();

            foreach (Element elem in elementTypes)
            {
                ElementType elementType = elem as ElementType;

                Parameter lengthParam = elementType.LookupParameter("ADSK_Размер_Ширина");
                Parameter heightParam = elementType.LookupParameter("ADSK_Размер_Высота");
                Parameter longParam = elementType.LookupParameter("ADSK_Размер_Длина");
                Parameter widthParam = elementType.LookupParameter("Толщина");
                Parameter diamParam = elementType.LookupParameter("ADSK_Размер_Диаметр");

                string materialNames = GetMaterialNames(elem, doc);
                string newName = FormNewName(category, lengthParam, heightParam, longParam, widthParam, diamParam, materialNames);

                if (!IsNameFormattedCorrectly(elementType.Name, newName))
                {
                    elementType.Name = newName;
                }
            }
        }

        #endregion

        #region Метод котороый создаёт новое имя типоразмеров 

        private string FormNewName(BuiltInCategory category, Parameter lengthParam, Parameter heightParam, Parameter longParam, Parameter widthParam, Parameter diamParam, string materialNames)
        {
            string length = lengthParam?.AsValueString();
            string height = heightParam?.AsValueString();
            string long1 = longParam?.AsValueString();
            string width = widthParam?.AsValueString();
            string diameter = diamParam?.AsValueString();


            if (category == BuiltInCategory.OST_StructuralFraming) // Вид имени типоразмера балок 
            {
                return $"{length} x {height}(h)_{materialNames}";
            }

            else if ( category == BuiltInCategory.OST_StructuralColumns) // Вид  имени  типоразмера колонн
                { 
                    if ( diameter != null)
                    {
                        return $"{diameter}(d)_{materialNames}";
                    }
                    else if ( diameter == null)
                    {
                    return $"{length} x {long1}(h)_{materialNames}";
                    }
                    else
                    {
                        return $"{length} x {long1}(h)_{materialNames}";
                    }
                }

            else if (category == BuiltInCategory.OST_Floors)  // Вид имени  типоразмера плит перекрытия
            {
                return $"{width}(h)_{materialNames}";
            }
            else if (category == BuiltInCategory.OST_Walls) // Вид имени типоразмера стен
            {
                return $"{width}(h)_{materialNames}";
            }
            else if (category == BuiltInCategory.OST_Windows) // Вид имени типоразмера окон
            {
                return $"{length} x {height}(h)";
            }
            else if (category == BuiltInCategory.OST_Doors) // Вид имени типоразмера дверей
            {
                return $"{length} x {height}(h)";
            }

            else
            {
                return $"{length} x {height}(h)";
            }





        }
        #endregion

        #region Метод , проверяющий изменилось ли имя типоразмера 

        private bool IsNameFormattedCorrectly(string currentName, string expectedName)
        {
            return currentName.Equals(expectedName);
        }
        #endregion

        # region Метод ,который поулчает парметры материала из элементов 
        private string GetMaterialNames(Element element, Document doc)
        {
            Parameter materialParam = element.LookupParameter("Материал несущих конструкций");

            if (materialParam != null && materialParam.HasValue)
            {
                ElementId materialId = materialParam.AsElementId();
                Material material = doc.GetElement(materialId) as Material;
                if (material != null)
                {
                    return material.Name;
                }
            }
            return "Материал несущих конструкций не найден";
        }
        #endregion
    }
}
