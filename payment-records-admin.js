(() => {
  const toast=document.getElementById('toast');
  const rows=document.getElementById('paymentRows');
  let toastTimer;
  let activeTab='all';
  let activeRecordId=null;
  const records=[
    {ref:'PAY-00182',taskId:'TSK-1048',task:'Fix a leaking faucet',member:'Maria Santos',method:'Cash',amount:600,date:'2026-10-08',status:'paid',category:'Repairs',area:'Brgy. 176',note:'Payment record submitted by a task member after task completion.',flagged:false,internalNote:''},
    {ref:'PAY-00179',taskId:'TSK-1049',task:'Grocery Shopping',member:'Juan Dela Cruz',method:'GCash',amount:450,date:'2026-10-06',status:'paid',category:'Shopping',area:'Brgy. 179',note:'Member-reported amount and payment status for a completed task.',flagged:false,internalNote:''},
    {ref:'PAY-00171',taskId:'TSK-1039',task:'Apartment Cleaning',member:'Maria Santos',method:'Cash',amount:550,date:'2026-10-03',status:'paid',category:'Cleaning',area:'Brgy. 178',note:'Member-reported amount and payment status for a completed task.',flagged:false,internalNote:''},
    {ref:'PAY-00165',taskId:'TSK-1046',task:'Math Tutor (Grade 10)',member:'Roberto Reyes',method:'Maya',amount:300,date:'2026-09-29',status:'paid',category:'Tutoring',area:'Brgy. 178',note:'Member-reported amount and payment status for a completed task.',flagged:false,internalNote:''},
    {ref:'PAY-00158',taskId:'TSK-1035',task:'Move a small cabinet',member:'Carlo Mendoza',method:'Bank transfer',amount:950,date:'2026-09-22',status:'paid',category:'Others',area:'Brgy. 176',note:'Member-reported amount and payment status for a completed task.',flagged:false,internalNote:''},
    {ref:'PAY-00149',taskId:'TSK-1042',task:'Pet Care',member:'Ana Cruz',method:'Cash',amount:400,date:'2026-09-18',status:'pending',category:'Pet Care',area:'Brgy. 176',note:'Payment record is still marked pending by the task member.',flagged:false,internalNote:''}
  ];
  function notify(message){toast.textContent=message;toast.classList.add('show');clearTimeout(toastTimer);toastTimer=setTimeout(()=>toast.classList.remove('show'),2800);}
  function safe(value){return String(value).replace(/[&<>"']/g,char=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[char]));}
  function prettyDate(value){return new Date(`${value}T00:00:00`).toLocaleDateString('en-PH',{month:'short',day:'numeric',year:'numeric'});}
  function methodIcon(method){return method==='Cash'?'₱':method==='GCash'?'G':method==='Maya'?'M':method==='Bank transfer'?'↗':'•';}
  function statusText(status){return status==='partially-paid'?'Partially paid':status[0].toUpperCase()+status.slice(1);}
  function getRecords(){
    const query=document.getElementById('paymentSearch').value.trim().toLowerCase();
    const method=document.getElementById('methodFilter').value;
    const status=document.getElementById('statusFilter').value;
    const sort=document.getElementById('paymentSort').value;
    let list=records.filter(record=>{
      const search=!query||`${record.ref} ${record.task} ${record.taskId} ${record.member} ${record.method} ${record.category}`.toLowerCase().includes(query);
      const tabMatches=activeTab==='all'||(activeTab==='flagged'?record.flagged:record.status===activeTab);
      return search&&tabMatches&&(method==='all'||record.method===method)&&(status==='all'||record.status===status);
    });
    if(sort==='recent')list.sort((a,b)=>b.date.localeCompare(a.date));
    if(sort==='oldest')list.sort((a,b)=>a.date.localeCompare(b.date));
    if(sort==='amount-high')list.sort((a,b)=>b.amount-a.amount);
    if(sort==='amount-low')list.sort((a,b)=>a.amount-b.amount);
    return list;
  }
  function render(){
    const list=getRecords();
    rows.innerHTML=list.map(record=>`<tr><td><div class="payment-reference"><span class="payment-ref-icon">₱</span><span><strong>${safe(record.ref)}</strong><small>Member-recorded</small></span></div></td><td><div class="payment-task-link"><a class="payment-task-link" href="[Admin] task-management.html?task=${encodeURIComponent(record.taskId)}">${safe(record.task)}</a><small>${safe(record.taskId)} · ${safe(record.category)} · ${safe(record.area)}</small></div></td><td><span class="payment-method"><i class="method-dot">${methodIcon(record.method)}</i>${safe(record.method)}</span></td><td class="payment-amount-cell">₱${record.amount.toLocaleString()}<small>Recorded amount</small></td><td>${prettyDate(record.date)}</td><td><span class="record-status ${safe(record.status)}">${safe(statusText(record.status))}</span></td><td><span class="review-flag ${record.flagged?'flagged':''}">${record.flagged?'⚑ Flagged':'Clear'}</span></td><td><button class="payment-row-action" data-open-record="${safe(record.ref)}">View</button></td></tr>`).join('');
    document.getElementById('paymentEmpty').hidden=list.length>0;
    document.querySelector('.admin-payment-table').hidden=list.length===0;
    document.getElementById('paymentResult').textContent=`Showing ${list.length} sample record${list.length===1?'':'s'}`;
    document.getElementById('paymentFooterCount').textContent=`${records.length} sample records`;
    document.getElementById('clearPaymentFilters').hidden=!(document.getElementById('paymentSearch').value||activeTab!=='all'||document.getElementById('methodFilter').value!=='all'||document.getElementById('statusFilter').value!=='all');
    const paid=records.filter(record=>record.status==='paid').length;
    const pending=records.filter(record=>record.status==='pending').length;
    const partial=records.filter(record=>record.status==='partially-paid').length;
    const flagged=records.filter(record=>record.flagged).length;
    const total=records.reduce((sum,record)=>sum+record.amount,0);
    document.getElementById('totalRecords').textContent=records.length;
    document.getElementById('totalAmount').textContent=`₱${total.toLocaleString()}`;
    document.getElementById('paidCount').textContent=paid;
    document.getElementById('pendingCount').textContent=pending;
    document.getElementById('paymentTabAll').textContent=records.length;
    document.getElementById('paymentTabPaid').textContent=paid;
    document.getElementById('paymentTabPending').textContent=pending;
    document.getElementById('paymentTabPartial').textContent=partial;
    document.getElementById('paymentTabFlagged').textContent=flagged;
  }
  function closeModals(){document.querySelectorAll('.modal-backdrop').forEach(modal=>modal.hidden=true);activeRecordId=null;}
  function openRecord(ref){
    const record=records.find(item=>item.ref===ref);if(!record)return;
    activeRecordId=record.ref;
    document.getElementById('paymentModalContent').innerHTML=`<div class="payment-modal-head"><span>₱</span><span><h2 id="paymentModalTitle">Payment record</h2><p>${safe(record.ref)} · Member-entered record</p></span></div><div class="payment-modal-amount"><span>Recorded amount</span><strong>₱${record.amount.toLocaleString()}</strong></div><div class="payment-detail-grid"><div class="payment-detail-item"><span>Task</span><strong><a href="[Admin] task-management.html?task=${encodeURIComponent(record.taskId)}">${safe(record.task)}</a></strong></div><div class="payment-detail-item"><span>Task reference</span><strong>${safe(record.taskId)}</strong></div><div class="payment-detail-item"><span>Member associated</span><strong>${safe(record.member)}</strong></div><div class="payment-detail-item"><span>Payment method</span><strong>${safe(record.method)}</strong></div><div class="payment-detail-item"><span>Member-reported status</span><strong>${safe(statusText(record.status))}</strong></div><div class="payment-detail-item"><span>Record date</span><strong>${prettyDate(record.date)}</strong></div><div class="payment-detail-item"><span>Category and area</span><strong>${safe(record.category)} · ${safe(record.area)}</strong></div><div class="payment-detail-item"><span>Admin review flag</span><strong>${record.flagged?'Flagged for review':'No flag'}</strong></div></div><div class="payment-modal-note"><strong>Record only</strong><br>This is a member-entered payment record. TaskTap did not process, hold, or transfer this amount.</div>${record.internalNote?`<div class="payment-internal-note"><strong>ADMIN REVIEW NOTE</strong>${safe(record.internalNote)}</div>`:''}<div class="payment-modal-actions"><button class="button secondary" data-close-modal>Close</button><button class="button ${record.flagged?'primary':'secondary'}" id="toggleFlag">${record.flagged?'Clear review flag':'Flag for review'}</button></div>`;
    document.getElementById('paymentModal').hidden=false;
    document.querySelectorAll('#paymentModalContent [data-close-modal]').forEach(button=>button.addEventListener('click',closeModals));
    document.getElementById('toggleFlag').addEventListener('click',()=>openFlagDialog(record));
  }
  function openFlagDialog(record){
    closeModals();activeRecordId=record.ref;
    const isFlagged=record.flagged;
    document.getElementById('flagModalTitle').textContent=isFlagged?'Clear this review flag?':'Flag this record for review?';
    document.getElementById('flagModalCopy').textContent=isFlagged?'This removes the demo flag only. The member-entered payment record remains unchanged.':'This adds a review flag to the sample record only. It does not change the payment status or move money.';
    document.getElementById('flagNote').value=record.internalNote||'';
    document.getElementById('confirmFlag').textContent=isFlagged?'Clear flag':'Flag record';
    document.getElementById('confirmFlag').className=isFlagged?'button secondary':'button primary';
    document.getElementById('flagModal').hidden=false;
  }
  function exportCSV(){
    const rows=[['Reference','Task','Task ID','Member','Method','Recorded amount (PHP)','Member-reported status','Date','Admin review flag'],...records.map(record=>[record.ref,record.task,record.taskId,record.member,record.method,record.amount,statusText(record.status),record.date,record.flagged?'Flagged':'Clear'])];
    const csv=rows.map(row=>row.map(value=>`"${String(value).replaceAll('"','""')}"`).join(',')).join('\r\n');
    const url=URL.createObjectURL(new Blob([csv],{type:'text/csv;charset=utf-8'}));const anchor=document.createElement('a');anchor.href=url;anchor.download='tasktap-demo-payment-records.csv';document.body.appendChild(anchor);anchor.click();anchor.remove();URL.revokeObjectURL(url);notify('Sample payment records exported as CSV.');
  }
  document.querySelectorAll('.future-link').forEach(link=>link.addEventListener('click',event=>{event.preventDefault();notify(`${link.dataset.page||'This page'} is planned for a future admin milestone.`);}));
  document.querySelectorAll('[data-toast]').forEach(link=>link.addEventListener('click',event=>{event.preventDefault();notify(link.dataset.toast);}));
  ['helpButton','notificationButton','profileButton','adminProfile'].forEach(id=>document.getElementById(id).addEventListener('click',()=>notify(id==='notificationButton'?'No new payment record alerts in this demo.':'Admin Demo account controls are not connected.')));
  document.querySelectorAll('.payment-tab').forEach(tab=>tab.addEventListener('click',()=>{activeTab=tab.dataset.status;document.querySelectorAll('.payment-tab').forEach(item=>{const selected=item===tab;item.classList.toggle('active',selected);item.setAttribute('aria-selected',selected?'true':'false');});render();}));
  ['paymentSearch','methodFilter','statusFilter','paymentSort'].forEach(id=>document.getElementById(id).addEventListener(id==='paymentSearch'?'input':'change',render));
  document.getElementById('clearPaymentFilters').addEventListener('click',()=>{document.getElementById('paymentSearch').value='';document.getElementById('methodFilter').value='all';document.getElementById('statusFilter').value='all';activeTab='all';document.querySelectorAll('.payment-tab').forEach(tab=>{const selected=tab.dataset.status==='all';tab.classList.toggle('active',selected);tab.setAttribute('aria-selected',selected?'true':'false');});render();});
  rows.addEventListener('click',event=>{const button=event.target.closest('[data-open-record]');if(button)openRecord(button.dataset.openRecord);});
  document.getElementById('exportPayments').addEventListener('click',exportCSV);
  document.getElementById('confirmFlag').addEventListener('click',()=>{const record=records.find(item=>item.ref===activeRecordId);if(!record)return;record.flagged=!record.flagged;record.internalNote=document.getElementById('flagNote').value.trim();const flagged=record.flagged;closeModals();render();notify(`${record.ref} ${flagged?'flagged for review':'flag cleared'} in this demo.`);});
  document.querySelectorAll('[data-close-modal]').forEach(button=>button.addEventListener('click',closeModals));
  document.querySelectorAll('.modal-backdrop').forEach(modal=>modal.addEventListener('click',event=>{if(event.target===modal)closeModals();}));
  document.addEventListener('keydown',event=>{if(event.key==='Escape')closeModals();if((event.ctrlKey||event.metaKey)&&event.key.toLowerCase()==='k'){event.preventDefault();document.getElementById('paymentSearch').focus();}});
  const sidebar=document.getElementById('sidebar');const scrim=document.getElementById('sidebarScrim');
  function closeSidebar(){sidebar.classList.remove('open');scrim.classList.remove('open');}
  document.getElementById('mobileMenu').addEventListener('click',()=>{sidebar.classList.toggle('open');scrim.classList.toggle('open');});scrim.addEventListener('click',closeSidebar);
  render();
})();
