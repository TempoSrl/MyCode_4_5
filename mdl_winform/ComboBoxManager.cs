using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using q  = mdl.MetaExpression;
using mdl;
using System.Data;
using mdl_utils;

namespace mdl_winform {

    /// <summary>
    /// Manages a combobox 
    /// </summary>
    public interface IComboBoxManager :IFormInit{
        /// <summary>
        /// Attach events to a combo
        /// </summary>
        /// <param name="c"></param>
        void addEvents(ComboBox c);

        /// <summary>
        /// enlist events to an eventManager
        /// </summary>
        /// <param name="eventManager"></param>
        void registerToEventManager(IFormEventsManager eventManager);

        /// <summary>
        /// Update a combobox when a row has been selected 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="changed"></param>
        /// <param name="changedRow"></param>
        void fillRelatedToRowControl(
            //HelpForm.drawmode dMode,
            //bool comboBoxToRefilter,
            ComboBox c,
            DataTable changed,
            DataRow changedRow);

        /// <summary>
        /// Gets the tip linked to a combo
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        string comboTip(ComboBox c);

        /// <summary>
        /// Prefills a combo with a specified filter, optionally changing its value
        /// </summary>
        /// <param name="c"></param>
        /// <param name="filter"></param>
        /// <param name="freshvalue"></param>
        void filteredPreFillCombo(ComboBox c, q filter, bool freshvalue);

        /// <summary>
        /// Fills a combobox with related data. It adds a dummy empty row to 
        ///  the PARENT table if the master selector allows null. This row
        ///  is marked as temp_row
        /// </summary>
        /// <remarks>combobox Tag field should be set to a "Table.column" string
        ///  that has to be displayed on that box. The table should be a PARENT
        ///  of the PrimaryTable linked to GetData object. 
        ///  More, the table have not to be a CHILD table itself
        /// </remarks>
        /// <param name="c"></param>
        /// <param name="filter">filter to apply</param>
        /// <param name="freshvalue">true if a RowChange Should be generated</param>
        /// <param name="selList"></param>
        void filteredPreFillCombo(ComboBox c, q filter, bool freshvalue, List<SelectBuilder> selList);

        /// <summary>
        /// Fill the table  related to a combobox.
        /// </summary>
        /// <param name="c">ComboBox to fill</param>
        /// <param name="freshvalue">when true, a redraw of combobox table related fields
        ///  is forced</param>
        void fillComboBoxTable(ComboBox c, bool freshvalue);

        /// <summary>
        /// Reimposta il ComboBox C per far si che contenga le voci di search
        /// </summary>
        /// <param name="c"></param>
        /// <param name="tablename"></param>
        void resetComboBoxSource(ComboBox c, string tablename);

        /// <summary>
        /// Reimposta il ComboBox C per far si che contenga tutte le voci operative in inserimento
        /// </summary>
        /// <param name="c"></param>
        /// <param name="tablename"></param>
        void refilterComboBoxSource(ComboBox c, string tablename);

        /// <summary>
        /// Imposta il ComboBox C con un sotto insieme delle righe della tabella principale del combo
        /// </summary>
        /// <param name="c"></param>
        /// <param name="combotable"></param>
        /// <param name="filter"></param>
        void setComboBoxFilteredSource(ComboBox c, string combotable, q filter);

        /// <summary>
        /// Imposta il DataSource del Combo in modo adeguato alla modalità corrente ed in modo che
        ///  possa visualizzare il valore OldValue
        /// </summary>
        /// <param name="c"></param>
        /// <param name="oldValue"></param>        
        void checkComboBoxSource(ComboBox c, object oldValue);

        /// <summary>
        /// Set the value of a specified ComboBox - given the DS DataSet
        /// The Tag of the ComboBox Should be like "masterfield[:parenttable.parentfield]" 
        /// </summary>
        /// <param name="c">ComboBox to fill</param>
        /// <param name="T"></param>
        /// <param name="field">field value to consider for combo</param>
        /// <param name="val"></param>
        void setCombo(ComboBox c, DataTable T, string field, object val);

        /// <summary>
        /// Sets the value of a combo
        /// </summary>
        /// <param name="c"></param>
        /// <param name="s"></param>
        void setComboBoxValue(ComboBox c, object s);

        /// <summary>
        /// Sets the value of a combo
        /// </summary>
        /// <param name="c"></param>
        /// <param name="s"></param>
        void setComboBoxStringValue(ComboBox c, string s);

        /// <summary>
        /// Clear combobox selected value
        /// </summary>
        /// <param name="c"></param>
        /// <param name="t"></param>
        void clearCombo(ComboBox c, DataTable t);                                
    }

    /// <summary>
    /// Implementation of an IComboBoxManager
    /// </summary>
    public class ComboBoxManager : IComboBoxManager {
        /// <summary>
        /// Attached eventsManager
        /// </summary>
        public IFormEventsManager eventsManager;

        static IMetaModel model = MetaFactory.factory.getSingleton<IMetaModel>();

        /// <summary>
        /// Adds events to the specified manager
        /// </summary>
        /// <param name="eventManager"></param>
        public void registerToEventManager(IFormEventsManager eventManager) {
            eventsManager = eventManager;
            eventManager.addListener(new ApplicationEventHandlerDelegate<StartMainRowSelectionEvent>(startMainRowSelection));
            eventManager.addListener(new ApplicationEventHandlerDelegate<StopMainRowSelectionEvent>(stopMainRowSelection));
            eventManager.addListener(new ApplicationEventHandlerDelegate<StartClearMainRowEvent>(startClearMainRow));
            eventManager.addListener(new ApplicationEventHandlerDelegate<StopClearMainRowEvent>(stopClearMainRow));
            eventManager.addListener(new ApplicationEventHandlerDelegate<ChangeFormState>(changeFormState));
        }

        private bool _comboBoxToRefill= true;
        private ApplicationFormState formState;

        private void startMainRowSelection(StartMainRowSelectionEvent e) {
            _comboBoxToRefill = true;
        }

        private void stopMainRowSelection(StopMainRowSelectionEvent e) {
            _comboBoxToRefill = false;
        }

        private void startClearMainRow(StartClearMainRowEvent e) {
            _comboBoxToRefill = true;
        }
        private void stopClearMainRow(StopClearMainRowEvent e) {
            _comboBoxToRefill = false;
        }

        private void changeFormState(ChangeFormState e) {
            formState = e.state;
        }
        private IGetData getData;
        private DataSet d;
        private IHelpForm helpForm;
        private IMessageShower shower;

        /// <summary>
        /// Manages a combobox
        /// </summary>
        public ComboBoxManager() { //IGetData getData, DataSet d,IHelpForm helpForm
           
            //this.getData = getData;
            //this.d = d;
            //this.helpForm = helpForm;
            
        }

        CQueryHelper qhc;  
        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        public void Init(Form f) {
            //f.attachInstance(this,typeof(IComboBoxManager));
            getData = f.getInstance<IGetData>();
            d = f.getInstance<DataSet>();
            helpForm = f.getInstance<IHelpForm>();
            eventsManager = f.getInstance<IFormEventsManager>();
            shower = MetaFactory.factory.getSingleton<IMessageShower>();
            qhc = MetaFactory.factory.getSingleton<CQueryHelper>();
        }
        /// <summary>
        /// Prefills a combo with a specified filter, optionally changing its value
        /// </summary>
        /// <param name="c"></param>
        /// <param name="filter"></param>
        /// <param name="freshvalue"></param>
        public void filteredPreFillCombo(ComboBox c, q filter, bool freshvalue) {
            filteredPreFillCombo(c, filter, freshvalue, null);
        }

        /// <summary>
        /// Fills a combobox with related data. It adds a dummy empty row to 
        ///  the PARENT table if the master selector allows null. This row
        ///  is marked as temp_row
        /// </summary>
        /// <remarks>combobox Tag field should be set to a "Table.column" string
        ///  that has to be displayed on that box. The table should be a PARENT
        ///  of the PrimaryTable linked to GetData object. 
        ///  More, the table have not to be a CHILD table itself
        /// </remarks>
        /// <param name="c"></param>
        /// <param name="filter">filter to apply</param>
        /// <param name="freshvalue">true if a RowChange Should be generated</param>
        /// <param name="selList"></param>
        public void filteredPreFillCombo(ComboBox c, q filter, 
            bool freshvalue,
            List<SelectBuilder> selList) {

            object oldValue = DBNull.Value;
            if (c.SelectedValue != null) oldValue = c.SelectedValue;

            if (!mdl_utils.tagUtils.CheckStandardTag(c.Tag)) return;
            var tag = tagUtils.GetStandardTag(c.Tag);

            var bind = tagUtils.GetLookup(tag);
            if (bind == null) return;

            //if (!CheckComboTag(C.Tag, parentcheck)) return;
            //C.Items.Clear();

            var T = ((DataTable)c.DataSource);
            if (PostData.IsTemporaryTable(T)) return;

            if (T == null) {
                shower.Show(c.FindForm(),
                    $"ComboBox {c.Name} in form {c.FindForm()?.Name} has not been linked to a DataSource");
                return;
            }
            //if (C.DropDownStyle!= ComboBoxStyle.DropDownList) C.DropDownStyle =  ComboBoxStyle.DropDownList; 
            var realTable = d.Tables[T.TableName];
            if (PostData.IsTemporaryTable(realTable)) return;


            var t2 = T;
            var ds2 = t2.DataSet;
            var combokind = comboTableKind(T) ?? "";

            if ((ds2 == null) || ! combokind.StartsWith("mkytemp") ) {
	            var clonehandle = mdl_utils.MetaProfiler.StartTimer("Cloning table * "+T.TableName);
                ds2 = new DataSet("mkytemp");
                //int clonehandle= metaprofiler.StartTimer("Cloning table "+T.TableName);
                ds2.EnforceConstraints = false;
                t2 = DataAccess.singleTableClone(T, true); //T.Clone();
                setComboTableKind(t2, "mkytemp");
                t2.ExtendedProperties["sort_by"] = T.ExtendedProperties["sort_by"];
                ds2.Tables.Add(t2);
                mdl_utils.MetaProfiler.StopTimer(clonehandle);
            }

            //var valueMember = c.ValueMember;
            //var displayMember = c.DisplayMember;


            //Checks that the table is not a child of another table. Infact in that case,
            // the list will be built depending of the selected row of the other table
            if ((filter is null) && (realTable.ParentRelations.Count > 0)) {
                eventsManager.DisableAutoEvents();
                if (c.DataSource != t2) {
                    //c.SuspendLayout();
                    //c.ValueMember = valueMember;
                    //c.DisplayMember = displayMember;
                    c.DataSource = t2;
                    //c.ResumeLayout();
                }
                eventsManager.EnableAutoEvents();
                return;
            }

            if (filter == null) {
                //set the table as "to cache"
                if (!model.IsCached(realTable)) {   //Mark table as cached only if it is not already. The point is that a locked-read table is also considered a cached table
                    model.CacheTable(realTable);
                }
            }

            eventsManager.DisableAutoEvents();

            model.MarkToAddBlankRow(realTable);
            model.MarkToAddBlankRow(t2);

            var sortField = c.DisplayMember;
            var pos = sortField.LastIndexOf('.');
            sortField = sortField.Substring(pos + 1);
            if (T.getSorting() != null) sortField = T.getSorting();

            var fc = new FillComboContext {
                C = c,
                T2 = t2,
                ValueMember = c.ValueMember,
                DisplayMember = c.DisplayMember,
                Freshvalue = freshvalue,
                OldValue = oldValue
            };
            //fc.state = formState;
           
            c.DataSource = null;
            c.DataBindings.Clear();
            var sel = getData.GetTable(realTable, sortBy:sortField, filter:filter, selList: selList, clear:true)
                                    .GetAwaiter().GetResult();
            c.DataSource = realTable;
            eventsManager.EnableAutoEvents();
            if (sel == null) {
                fillComboBoxTable(realTable, fc);
            }
            else {
                sel.AddOnRead(fillComboBoxTable, fc);
            }

        }

        class FillComboContext {
            internal ComboBox C;
            internal DataTable T2;
            internal string ValueMember;
            internal string DisplayMember;
            internal bool Freshvalue;
            internal object OldValue; 

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="realTable"></param>
        /// <param name="context"></param>
        public void fillComboBoxTable(DataTable realTable, object context) {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var ctx = context as FillComboContext;
            var c = ctx.C;
            var t2 = ctx.T2;
            eventsManager.DisableAutoEvents();
            c.SelectedIndex = -1;
            
            c.BeginUpdate();
            c.SuspendLayout();
            c.DataSource = null;
            c.DataBindings.Clear();
            model.Clear(t2);

            if (formState == ApplicationFormState.Empty) {
               
                var searchfilter = QueryCreator.GetSearchFilter(realTable);
                if (searchfilter == null) {
                    DataSetUtils.MergeDataTable(t2, realTable);
                    setComboTableKind(t2, "mkytemp");
                }
                else {
                    DataSetUtils.MergeDataTable(t2, realTable);
                    foreach (var r in t2.Select("not(" + searchfilter + ")")) {
                        t2.Rows.Remove(r);
                    }
                    setComboTableKind(t2, "mkytemp");
                }               
            }

            if (formState != ApplicationFormState.Empty) {                
                var insertfilter = QueryCreator.GetInsertFilter(realTable);
                if (insertfilter == null) {
                    DataSetUtils.MergeDataTable(t2, realTable);
                    setComboTableKind(t2, "mkytemp");
                }
                else {
                    DataSetUtils.MergeDataTable(t2, realTable);
                    foreach (var r in t2.Select("not(" + insertfilter + ")")) {
                        t2.Rows.Remove(r);
                    }
                    setComboTableKind(t2, "mkytemp_insert");
                }               

            }

            if (c.DataSource != t2) {               
                c.ValueMember = ctx.ValueMember;
                c.DisplayMember = ctx.DisplayMember;
                c.DataSource = t2;               
            }

            
            c.EndUpdate();
            
            c.ResumeLayout();
         
            if (ctx.Freshvalue) {
                eventsManager.EnableAutoEvents();
                //MarkEvent("Events UNLOCKED...");
            }

            //This always generates a call to fillrelatedrows, cause now 
            c.SelectedIndex = -1; //(table has been cleared)
           
            try {
                if (ctx.Freshvalue) {
                    c.SelectedIndex = 0;
                }
                else {
                    checkComboBoxSource(c, ctx.OldValue);
                    SetComboBoxValue(c, ctx.OldValue);
                }
            }
            catch {
                // ignored
            }


            if (ctx.Freshvalue && (c.SelectedIndex == 0)) {

                //verificare che questa istruzione che ho rimosso non fosse necessaria (e magari dire perchè)
                //risposta: necessaria per aggiornare i combo dipendenti
                helpForm.IterateFillRelatedControls(c.Parent.Controls, c, realTable, null);

            }

            if (!ctx.Freshvalue) {
                eventsManager.EnableAutoEvents();
                //MarkEvent("Events UNLOCKED...");
            }
        }


        /// <summary>
        /// Fill the table  related to a combobox.
        /// </summary>
        /// <param name="C">ComboBox to fill</param>
        /// <param name="freshvalue">when true, a redraw of combobox table related fields
        ///  is forced</param>
        public void fillComboBoxTable(ComboBox C, bool freshvalue) {        
            if (!tagUtils.CheckStandardTag(C.Tag)) return;
            var tag = tagUtils.GetStandardTag(C.Tag);

            var bind = tagUtils.GetLookup(tag);
            if (bind == null) return;
                                 
            var T = ((DataTable)C.DataSource);

            if (T == null) {
                var err = "ComboBox " + C.Name + " ";
                var ff = C.FindForm();
                if (ff != null) err += " in form " + ff.Name + " ";
                err += " has not been linked to a DataSource";
                shower.Show(ff, err);
                return;
            }
            var handle = mdl_utils.MetaProfiler.StartTimer("In FillComboBoxTable");
            if (C.DropDownStyle != ComboBoxStyle.DropDown)
                C.DropDownStyle = ComboBoxStyle.DropDown;

            var realTable = d.Tables[T.TableName];

            var t2 = T;
            var combokind = comboTableKind(t2)??"";
            if (!combokind.StartsWith("mkytemp")) {
                var ds2 = new DataSet("mkytemp") {EnforceConstraints = false};
                t2 = DataAccess.singleTableClone(T, true); 
                setComboTableKind(t2, "mkytemp_insert");
                t2.ExtendedProperties["sort_by"] = T.ExtendedProperties["sort_by"];
                ds2.Tables.Add(t2);
            }

            eventsManager.DisableAutoEvents();

            //C.SuspendLayout();
            //C.BeginUpdate();
            
            var valueMember = C.ValueMember;
            var displayMember = C.DisplayMember;
            C.DataSource = null;
            C.DataBindings.Clear();
            model.Clear(realTable);   
            model.CheckBlankRow(realTable);

           
          
          
            

            C.SelectedIndex = -1;
          
            model.Clear(t2);
        

            DataSetUtils.MergeDataTable(t2, realTable);

            //			MarkEvent("Before DataSource=T2");
            C.DisplayMember = displayMember;
            C.ValueMember = valueMember;
            C.DataSource = t2;

            //			MarkEvent("After DataSource=T2");
            //C.EndUpdate();
            //C.ResumeLayout();
            // (*) it was here 

        

            eventsManager.EnableAutoEvents();
            checkComboBoxSource(C, DBNull.Value);
            //			MarkEvent("Events UNLOCKED...");

            //This always generates a call to fillrelatedrows, cause now  C.SelectedIndex=-1 (table has been cleared)
            if (freshvalue) {
                // NOT_TODO: verificare che questa istruzione che ho rimosso non fosse necessaria (e magari dire perchè)
                //E' necessaria per aggiornare, per esempio, i combo dipendenti (!)
                helpForm.IterateFillRelatedControls(C.Parent.Controls, C, realTable, null);
            }
            mdl_utils.MetaProfiler.StopTimer(handle);

        }
        /// <summary>
        /// Get the kind of a table viewed in a combobox
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        string comboTableKind(DataTable T) {
            return T.ExtendedProperties["ComboTableKind"] as string;
        }

        /// <summary>
        /// Set the kind of a table viewed in a combobox
        /// </summary>
        /// <param name="T"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        void setComboTableKind(DataTable T, string kind) {
            T.ExtendedProperties["ComboTableKind"] = kind;
        }

        /// <summary>
        /// Reimposta il ComboBox C per far si che contenga le voci di search
        /// </summary>
        /// <param name="c"></param>
        /// <param name="tablename"></param>
        public void resetComboBoxSource(ComboBox c, string tablename) {

            var T = d.Tables[tablename];

            if (formState == ApplicationFormState.Insert && QueryCreator.GetInsertFilter(T) != null) {
                refilterComboBoxSource(c, tablename);
                return;
            }

            if (PostData.IsTemporaryTable(T)) return;

            var t2 = (DataTable)(c.DataSource);
            if (comboTableKind(t2) == "mkytemp") return;

            //Reimposta il DataSource ---> 
            setComboBoxFilteredSource(c, tablename, QueryCreator.GetSearchFilter(T));
            setComboTableKind(t2, "mkytemp");

        }

        /// <summary>
        /// Reimposta il ComboBox C per far si che contenga tutte le voci operative in inserimento
        /// </summary>
        /// <param name="c"></param>
        /// <param name="tablename"></param>
        public void refilterComboBoxSource(ComboBox c, string tablename) {
            var T = d.Tables[tablename];
            if (PostData.IsTemporaryTable(T)) return;

            var t2 = (DataTable)(c.DataSource);
            if (comboTableKind(t2) == "mkytemp_insert") return;

            //Reimposta il DataSource
            setComboBoxFilteredSource(c, tablename, QueryCreator.GetInsertFilter(T));
            setComboTableKind(t2, "mkytemp_insert");
        }


        /// <summary>
        /// Imposta il ComboBox C con un sotto insieme delle righe della tabella principale del combo
        /// </summary>
        /// <param name="c"></param>
        /// <param name="combotable"></param>
        /// <param name="filter"></param>
        public void setComboBoxFilteredSource(ComboBox c, string combotable, q filter) {

            eventsManager.DisableAutoEvents();

            var t2 = (DataTable)(c.DataSource);
            var realTable = d.Tables[combotable];
            if (PostData.IsTemporaryTable(realTable)) return;

            var j = mdl_utils.MetaProfiler.StartTimer("SetComboBoxFilteredSource");
            var oldval = c.ValueMember;
            var olddescr = c.DisplayMember;
            
            c.BeginUpdate();
            c.SuspendLayout();
            c.DataSource = null;
            c.DataBindings.Clear();
            

            c.SelectedIndex = -1;

            model.Clear(t2);
            //T2.Clear(); //NON SERVE A NULLA - INUTILE PROVARLA!!!
            if (filter == null) {
                DataSetUtils.MergeDataTable(t2, realTable);
            }
            else {
                DataSetUtils.MergeDataTable(t2, realTable);
                foreach (var r in t2.Select("not(" + filter.toADO() + ")")) {
                    r.Delete();
                }
                t2.AcceptChanges();
                //T2.DataSet.Merge(RealTable.Select(filter));
            }
            //T2.AcceptChanges();// INUTILE ANCHE QUESTA!

            mdl_utils.MetaProfiler.StopTimer(j);




            c.DisplayMember = olddescr;
            c.ValueMember = oldval;
            c.DataSource = t2;
            

            c.EndUpdate();
            c.ResumeLayout();
            
          
            eventsManager.EnableAutoEvents();
        }

        /// <summary>
        /// Imposta il DataSource del Combo in modo adeguato alla modalità corrente ed in modo che
        ///  possa visualizzare il valore OldValue
        /// </summary>
        /// <param name="c"></param>
        /// <param name="oldValue"></param>
        public void checkComboBoxSource(ComboBox c, object oldValue) {
            if (c.DataSource == null) return;
            var tt = ((DataTable)c.DataSource);

            var T = d.Tables[tt.TableName];
            if (T == null) return;
            var table = T.TableName;

            if (PostData.IsTemporaryTable(T)) return;

            //Se T non ha filtro pre l'inserimento non deve fare nulla
            if (QueryCreator.GetInsertFilter(T) == null) return;

            //Esaminiamo ora il caso in cui T HA filtro per insert. In questo caso può accadere che
            //  DataSetName = mkytemp_insert o mkytemp_special
            if (formState == ApplicationFormState.Empty) {
                resetComboBoxSource(c, table);
                return;
            }

            if (formState == ApplicationFormState.Insert) {
                refilterComboBoxSource(c, table);
                return;
            }

            //Modo è EDIT. 


            var fieldtoconsider = c.ValueMember;

            q oldvaluefilter = ((oldValue == DBNull.Value) || (oldValue == null))
                ? null
                : q.eq(fieldtoconsider, oldValue); //$"({fieldtoconsider}={mdl_utils.Quoting.quote(oldValue, false)})";
            var combokind = comboTableKind(tt);
            //Se il source è mkytemp_insert e odlvalue non è incluso, lo deve aggiungere
            // se invece il source non è mkytemp_insert e odlvalue è incluso in mkytemp_insert, deve 
            //  ripristinare mkytemp_insert
            if (combokind != "mkytemp_insert") {
                //se odlvalue è incluso in mkytemp_insert, è quello che bisogna usare per il combo.
                var keyfilter = q.and(oldvaluefilter, QueryCreator.GetFilterForInsert(T));
                if ((oldvaluefilter == null) || (tt.filter(keyfilter).Length > 0)) {
                    refilterComboBoxSource(c, table); //ripristina mkytemp_insert
                    return;
                }
                var keyandfilter = q.and(oldvaluefilter, QueryCreator.GetSearchFilter(T));
                if (tt.filter(keyandfilter).Length > 0) return;

                var keyorfilter = q.or(oldvaluefilter, QueryCreator.GetSearchFilter(T));
                //"(" + oldvaluefilter + ")OR(" + QueryCreator.GetSearchFilter(T) + ")";
                setComboBoxFilteredSource(c, table, keyorfilter);
                setComboTableKind(tt, "mkytemp_special");

                return;
            }

            if (oldvaluefilter == null) return;

            if (combokind == "mkytemp_insert") {
                //se R è incluso in mkytemp_insert, è quello che bisogna usare per il combo.
                var keyfilter = oldvaluefilter;
                var keyandfilter = q.and(keyfilter, QueryCreator.GetInsertFilter(T));
                q keyorfilter = q.or(keyfilter, QueryCreator.GetInsertFilter(T));
                //"("+keyfilter+")OR("+QueryCreator.GetInsertFilter(T)+")";
                if (tt.filter(keyandfilter).Length == 0) {
                    //Bisogna aggiungere R al combo, e cambiare il DataSet di nome
                    setComboBoxFilteredSource(c, table, keyorfilter);
                    setComboTableKind(tt, "mkytemp_special");
                }
                return;
            }

        }

        /// <summary>
        /// Sets the value for a combobox
        /// </summary>
        /// <param name="c"></param>
        /// <param name="s"></param>
        public void setComboBoxStringValue(ComboBox c, string s) {
            s = s?.ToUpper();
            SetComboBoxValue(c, (object) s);
        }

        /// <summary>
        /// Sets the value for a combobox
        /// </summary>
        /// <param name="c"></param>
        /// <param name="s"></param>
        internal static void SetComboBoxStringValue(ComboBox c, string s) {
            s = s?.ToUpper();
            SetComboBoxValue(c, (object) s);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="s"></param>
        public void setComboBoxValue(ComboBox c, object s) {
            if (_comboBoxToRefill) {
                checkComboBoxSource(c, s);
            }
            SetComboBoxValue(c,s); 
        }
     
        /// <summary>
        /// Sets the value of a ComboBox making it displaying (valuemember) S if possible,
        ///  disregarding case
        /// </summary>
        /// <param name="c">ComboBox to fill</param>
        /// <param name="valueToSet">string wanted as SelectedValue</param>
        internal static void SetComboBoxValue(ComboBox c, object valueToSet) {


            var fieldtoconsider = c.ValueMember;
            if (valueToSet == null) valueToSet = DBNull.Value;
            
            var first = -1;
            var typeok = false;
            DataTable comboTable = null;
            for (var i = 0; i < c.Items.Count; i++) {
                var v = (DataRowView)c.Items[i];
                comboTable = v.Row.Table;

                //converte valueToSet al tipo del DataColumn
                if (valueToSet != DBNull.Value && !typeok) {
                    typeok = true;

                    var columnType = comboTable.Columns[fieldtoconsider].DataType;
                    if (columnType != valueToSet.GetType()) {
                        if (columnType == typeof(string)) valueToSet = valueToSet.ToString();
                        if (columnType == typeof(int)) valueToSet = Convert.ToInt32(valueToSet);
                        if (columnType == typeof(byte)) {
                            if (Convert.ToInt32(valueToSet)>=0) valueToSet = Convert.ToByte(valueToSet);
                        }
                        if (columnType == typeof(uint)) {
                            if (Convert.ToInt32(valueToSet) >= 0) valueToSet = Convert.ToUInt32(valueToSet);
                        }
                        if (columnType == typeof(short)) valueToSet = Convert.ToInt16(valueToSet);
                        if (columnType == typeof(ushort)) {
                            if (Convert.ToInt32(valueToSet) >= 0) valueToSet = Convert.ToUInt16(valueToSet);
                        }
                    }
                }
                if ((v.Row.RowState == DataRowState.Deleted)
                    || (v.Row.RowState == DataRowState.Detached)) continue;
                if (first == -1) first = i;
                var testvalue = v[fieldtoconsider];
                if (testvalue.Equals(valueToSet)) {
                    if (c.SelectedIndex != i) {
                         c.SelectedIndex = i;
                    }
                    return;
                }
            }
           
            if ((c.Items.Count == 1) && (first >= 0)) {
                c.SelectedIndex = first;
                return;
            }
            if (comboTable == null) return;
            if (model.MarkedToAddBlankRow(comboTable) && (c.Items.Count > 0) && (first >= 0)) {
                c.SelectedIndex = first;
            }
           
        }



        /// <summary>
        /// Set the value of a specified ComboBox - given the DS DataSet
        /// The Tag of the ComboBox Should be like "masterfield[:parenttable.parentfield]" 
        /// </summary>
        /// <param name="c">ComboBox to fill</param>
        /// <param name="T"></param>
        /// <param name="field">field value to consider for combo</param>
        /// <param name="val"></param>
        public void setCombo(ComboBox c, DataTable T, string field, object val) {
            if (_comboBoxToRefill) {
                checkComboBoxSource(c, val);
            }

            var isDenyNull = (!T.Columns[field].AllowDBNull || T.Columns[field].IsDenyNull());

            if (c.Items.Count == 2
                && isDenyNull
                && formState == ApplicationFormState.Insert) {  //dmode == HelpForm.drawmode.insert
                c.SelectedIndex = 1;
                return;
            }

            if (!tagUtils.CheckStandardTag(c.Tag)) {
                c.SelectedIndex = c.Items.IndexOf(val);
                return;
            }

            if (val == null) {
                c.SelectedIndex = -1;
                return;
            }

            //calls the static method, so that checkComboBoxSource will not be called again
            SetComboBoxValue(c, val);
           
        }


        /// <summary>
        /// Update a combobox when a row has been selected 
        /// </summary>
        /// <param name="c">Combobox to (eventually) update</param>
        /// <param name="changed">Table from where a new row has been selected</param>
        /// <param name="changedRow">Row selected in some control</param>
        public void fillRelatedToRowControl(
            //HelpForm.drawmode dMode, 
            //bool comboBoxToRefilter,
            ComboBox c,
            DataTable changed,
            DataRow changedRow) {
            var changedName = changed.TableName;
            var tag = tagUtils.GetStandardTag(c.Tag);
            if (tag == null) return;

            var dataSource2 = (DataTable)c.DataSource;
            if (dataSource2 == null) {
                //MetaFactory.factory.getSingleton<IMessageShower>().Show(me+" has no DataSource");
                return;
            }
            var dataSource = d.Tables[dataSource2.TableName];

            var dataSourceName = dataSource.TableName;

            var tagTableName = tagUtils.GetTableName(tag);
            if (tagTableName == null) {
                shower.Show(c.FindForm(), HelpForm.mydescr(c) + " has not a valid Table in (Standard)Tag (" + tag + ")","Design time Error");
                return;
            }
            var tagTable = d.Tables[tagTableName];
            if (tagTable == null) {
                shower.Show(c.FindForm(), HelpForm.mydescr(c) + " has not a valid Table in (Standard) Tag (" + tag + ")","Design time Error");
                return;
            }


            if (changedName == dataSourceName) {
                //change come from same table as DataSource..but should come from a different control!
                var column = c.ValueMember;
                //The combobox is linked to a field from the same table as Parent control
                object newval = DBNull.Value;
                if (changedRow != null) newval = changedRow[column];
                //if (_comboBoxToRefill) checkComboBoxSource(c, newval, dMode);
                SetComboBoxValue(c, newval);
           
                return;
            }

           
            if (changedName == tagTableName) {
                var colname = tagUtils.GetColumnName(tag);
                object newval = DBNull.Value;
                if (changedRow != null) newval = changedRow[colname];
                //if (_comboBoxToRefill) checkComboBoxSource(c, newval, dMode);
                SetComboBoxValue(c, newval);                       
                return;
            }

            var foundRel = QueryCreator.GetParentChildRel(changed, dataSource);
            if (foundRel == null) return;
       
            //Parent Row of ComboBox is Changed --> Combo must be updated
            if (changedRow == null) {
                fillComboBoxTable(c, true);                  
                return;
            }
            var filter = q.mGetParents(changedRow, foundRel, DataRowVersion.Default );
                        //foundRel.ParentColumns, foundRel.ChildColumns, DataRowVersion.Default, true);
            //try {                  
            filteredPreFillCombo(c, filter, true);  //here a refresh of child must be generated                          
            //}
            //catch (Exception e) {
                
            //        //QueryCreator.ShowException((object) c.FindForm(), null,(Exception)  e);
            //}
            
        }


        /// <summary>
        /// Add internal events to manage combobox
        /// </summary>
        /// <param name="c"></param>
        public void addEvents(ComboBox c) {
            try {
                if (c.Tag != null && c.Tag.ToString() != "") {
                    if (c.DropDownStyle != ComboBoxStyle.DropDown)
                        c.DropDownStyle = ComboBoxStyle.DropDown;
                    c.MaxDropDownItems = 25;
                }
                else {
                    c.DropDownStyle = ComboBoxStyle.DropDownList;
                }
            }
            catch {
                //ignore
            }
            c.KeyDown += keyDown;
            c.KeyPress += keyPress;
        }

        /// <summary>
        /// Evento generato ad ogni pressione di tasto tale che "IsInputKey() = true"; 
        /// pertanto anche "ESC", "INVIO" e "BACKSPACE" ma non,
        /// ad esempio, "SINISTRA", "DESTRA", "HOME" e "CANC" che devono essere gestiti
        /// dall'evento "KeyDown".
        /// Precondizione: nel ComboBox DEVE ESSERE DropDownStyle = DropDown
        /// </summary>
        /// <param name="sender">il ComboBox che si vuole gestire</param>
        /// <param name="e">l'evento</param>
        private static void keyPress(object sender, KeyPressEventArgs e) {
            
            //Se è stato premuto ESC o INVIO lascio la gestione dell'evento a .NET
            if (e.KeyChar == 27 || e.KeyChar == 13) {
                return;
            }

            var acComboBox = (ComboBox)sender;

            var selectionStart = acComboBox.SelectionStart;
            var comboLen = acComboBox.Text.Length;
            var comboText = acComboBox.Text;
           
            if (selectionStart > comboLen) selectionStart = comboLen;
            if (selectionStart < 0) selectionStart = 0;

            //Se il tasto premuto è BACKSPACE, faccio cominciare la selezione un carattere prima
            //dell'inizio della selezione corrente
            if (e.KeyChar == 8) {
                var lenSel = comboLen - (selectionStart - 1);
                if (selectionStart > 0 && lenSel >= 0) {
                    acComboBox.Select(selectionStart - 1, lenSel);
                }
                else {
                    acComboBox.SelectAll();
                }
            }
            else {
                //Se è un qualunque altro carattere (quindi tale che IsInputKey()=true
                //e diverso anche da ESC, INVIO, BACK) allora lo gestisco io.

                //Cerco una riga del ComboBox che cominci per i primi "selectionStart" caratteri
                //della riga corrente concatenati col tasto premuto
                var ricerca = comboText.Substring(0, selectionStart) + e.KeyChar;

                var index = acComboBox.FindString(ricerca);

                if (index != -1) {
                    //Se tale riga esiste, allora la seleziono
                    if (acComboBox.SelectedIndex != index) {
                        acComboBox.DroppedDown = false;
                        acComboBox.SelectedIndex = index;                        
                        comboLen = acComboBox.Text.Length;                        
                        
                    }
                    acComboBox.DroppedDown = true;
                    if (selectionStart < comboLen) {
                        //e faccio cominciare la selezione da selectionstart + 1
                        acComboBox.Select(selectionStart + 1, comboLen - (selectionStart + 1));
                    }
                }
                else {
                    //MarkEvent("Ricerca riuscita");
                    //Se invece tale riga non esiste, allora seleziono la riga attuale
                    //dal carattere in posizione selectionStart fino alla fine
                    acComboBox.DroppedDown = true;
                    acComboBox.Select(selectionStart, comboLen - selectionStart);
                }
            }
            //Forzo l'apertura della tendina per facilitare l'utente nella scelta
            e.Handled = true;
        }

        /// <summary>
        /// Evento generato prima di KeyPress. Lo uso per gestire la pressione dei tasti 
        /// "SINISTRA", "DESTRA", "HOME" e "CANC"
        /// che altrimenti non riuscirei ad intercettare con KeyPress.
        /// Precondizione: nel ComboBox DEVE ESSERE DropDownStyle = DropDown
        /// </summary>
        /// <param name="sender">il ComboBox da gestire</param>
        /// <param name="e">l'evento</param>
        private static void keyDown(object sender, KeyEventArgs e) {            
            var acComboBox = (ComboBox)sender;
            var selectionStart = acComboBox.SelectionStart;

            switch (e.KeyCode) {
                //Se è stato premuta la freccia "SINISTRA" faccio cominciare la selezione
                //un carattere prima rispetto alla selezione attuale
                case Keys.Left:
                    if (selectionStart > 0) {
                        acComboBox.Select(selectionStart - 1, acComboBox.Text.Length - (selectionStart - 1));
                    }
                    else {
                        acComboBox.SelectAll();
                    }
                    break;

                //Se è stato premuto il tasto "CANC" seleziono la riga vuota del combobox
                case Keys.Delete:
                    var index = acComboBox.FindString("");
                    if (index != -1) {
                        acComboBox.DroppedDown = false;
                        acComboBox.SelectedIndex = index;
                        acComboBox.DroppedDown = true;
                    }
                    acComboBox.SelectAll();
                    break;

                //Se è stato premuta la freccia "DESTRA" faccio cominciare la selezione
                //un carattere dopo rispetto alla selezione attuale
                case Keys.Right:
                    if (acComboBox.Text.Length > selectionStart) {
                        acComboBox.Select(selectionStart + 1, acComboBox.Text.Length - (selectionStart + 1));
                    }
                    break;

                //Se è stato premuto il tasto "HOME" seleziono tutta la riga attuale.
                case Keys.Home:
                    acComboBox.SelectAll();
                    break;

                default:
                    //Altrimenti lascio la gestione di questo evento a .NET
                    return;
            }
            e.Handled = true;
        }


        /// <inheritdoc />
        public string comboTip(ComboBox c) {
            if (c.SelectedValue != null) {
                return "\nValue:" + c.SelectedValue;
            }
            return "\nValue: null";
        }


        /// <inheritdoc />
        public void clearCombo(ComboBox c,DataTable t) {
	        var s = mdl_utils.MetaProfiler.StartTimer($"clearCombo*({t?.TableName})");

	        if (_comboBoxToRefill) {
		        resetComboBoxSource(c, t.TableName);
	        }
            
            c.SelectedIndex = -1; 
			mdl_utils.MetaProfiler.StopTimer(s);
        }

    }
}
