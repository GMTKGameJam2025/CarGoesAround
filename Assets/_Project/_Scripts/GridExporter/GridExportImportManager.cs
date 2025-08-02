using System.IO;
using UnityEngine;
public class GridExportImportManager : MonoBehaviour
{
    [Header("Components")]
    public GridExporter exporter;
    public GridLoader loader;
    
    private void Awake()
    {
        if (exporter == null) exporter = GetComponent<GridExporter>();
        if (loader == null) loader = GetComponent<GridLoader>();
        
        if (exporter == null) exporter = gameObject.AddComponent<GridExporter>();
        if (loader == null) loader = gameObject.AddComponent<GridLoader>();
    }
    
    [ContextMenu("Quick Export")]
    public void QuickExport()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "GridData_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json");
        GridExportData data = exporter.ExportGrid();
        if (data != null)
        {
            exporter.SaveToFile(data, path);
            Debug.Log($"Quick export saved to: {path}");
        }
    }
}