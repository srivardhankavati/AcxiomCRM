namespace AcxiomCrm.ViewModels;
public class DashboardViewModel{public int TotalCustomers{get;set;}public int TotalLeads{get;set;}public int OpenLeads{get;set;}public int TotalOpportunities{get;set;}public int OpenOpportunities{get;set;}public int WonOpportunities{get;set;}public int LostOpportunities{get;set;}public decimal Pipeline{get;set;}public List<ChartItem> LeadStatus{get;set;}=[];public List<ChartItem> OpportunityStage{get;set;}=[];}
public class ChartItem{public string Label{get;set;}="";public int Count{get;set;}}
