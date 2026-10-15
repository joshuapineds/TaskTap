(() => {
  const toast=document.getElementById('toast');
  const rows=document.getElementById('reportRows');
  let toastTimer;
  let activeStatus='all';
  let pendingUpdate=null;
  const reports=[
    {id:'RPT-00982',type:'Task',targetId:'TSK-1048',targetName:'Fix a leaking kitchen faucet',reporter:'Maria Cruz',reporterInitials:'MC',reason:'Misleading listing',priority:'High',submitted:'12 min ago',date:'2026-10-11',status:'Open',summary:'The task description may not match the work being requested. The reporter says the posted details changed after they asked a question.',context:'The listing description and reported conversation should be reviewed together. No conclusion has been made in this demo.',note:''},
    {id:'RPT-00981',type:'User',targetId:'USR-2475',targetName:'Enzo Ramirez',reporter:'Jose Lim',reporterInitials:'JL',reason:'Harassment',priority:'High',submitted:'38 min ago',date:'2026-10-11',status:'Open',summary:'The reporter says they received repeated unwanted messages after declining a task request.',context:'Sample report text only. Review the relevant conversation before taking action.',note:''},
    {id:'RPT-00980',type:'Task',targetId:'TSK-1015',targetName:'Deliver lunch to a neighbor',reporter:'Angela Reyes',reporterInitials:'AR',reason:'Duplicate listing',priority:'Normal',submitted:'2 hrs ago',date:'2026-10-11',status:'Open',summary:'The reporter believes this listing duplicates another recent delivery task.',context:'Compare the task details and posting history in context.',note:''},
    {id:'RPT-00979',type:'User',targetId:'USR-2479',targetName:'Noel Garcia',reporter:'Ramon Dela Cruz',reporterInitials:'RD',reason:'Spam',priority:'Normal',submitted:'Yesterday',date:'2026-10-10',status:'Open',summary:'The reporter says they received repeated promotional messages unrelated to a task.',context:'Check the account activity and message context before acting.',note:''},
    {id:'RPT-00978',type:'Task',targetId:'TSK-1024',targetName:'Pick up prescription refill',reporter:'Grace Mendoza',reporterInitials:'GM',reason:'Off-platform payment',priority:'High',submitted:'Yesterday',date:'2026-10-10',status:'Open',summary:'The reporter is concerned that the task conversation requests payment details outside TaskTap.',context:'TaskTap does not process payments. Review the task-related conversation for policy context.',note:''},
    {id:'RPT-00977',type:'User',targetId:'USR-2481',targetName:'Rafael Dizon',reporter:'Liza Torres',reporterInitials:'LT',reason:'Other concern',priority:'Low',submitted:'Oct 9',date:'2026-10-09',status:'Open',summary:'The reporter has a concern about the accuracy of profile information.',context:'Sample concern. Verify context and avoid sharing personal information unnecessarily.',note:''},
    {id:'RPT-00976',type:'Task',targetId:'TSK-1032',targetName:'Garden cleanup and trimming',reporter:'Ben Castillo',reporterInitials:'BC',reason:'Misleading listing',priority:'Normal',submitted:'Oct 9',date:'2026-10-09',status:'Open',summary:'The scope and expected duration may not be clear from the task listing.',context:'Review the task description and member conversation for details.',note:''},
    {id:'RPT-00975',type:'User',targetId:'USR-2476',targetName:'Lara Domingo',reporter:'Nina Flores',reporterInitials:'NF',reason:'Unsafe behavior',priority:'Low',submitted:'Oct 8',date:'2026-10-08',status:'Open',summary:'The reporter says they felt uncomfortable with a request to share an exact home address too early.',context:'Location privacy is important. TaskTap displays barangay-level locations in public listings.',note:''}
  ];
  function notify(message){toast.textContent=message;toast.classList.add('show');clearTimeout(toastTimer);toastTimer=setTimeout(()=>toast.classList.remove('show'),2800);}
  function safe(value){return String(value).replace(/[&<>"']/g,char=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[char]));}
  function getHref(report){return report.type==='User'?`[Admin] user-detail.html?id=${encodeURIComponent(report.targetId)}`:`[Admin] task-management.html?task=${encodeURIComponent(report.targetId)}`;}
  function matches(){
    const query=document.getElementById('reportSearch').value.trim().toLowerCase();
    const type=document.getElementById('typeFilter').value;
    const reason=document.getElementById('reasonFilter').value;
    const priority=document.getElementById('priorityFilter').value;
    return reports.filter(report=>{
      const search=!query||`${report.id} ${report.type} ${report.targetName} ${report.targetId} ${report.reporter} ${report.reason} ${report.summary}`.toLowerCase().includes(query);
      return search&&(activeStatus==='all'||report.status===activeStatus)&&(type==='all'||report.type===type)&&(reason==='all'||report.reason===reason)&&(priority==='all'||report.priority===priority);
    }).sort((a,b)=>b.date.localeCompare(a.date)||b.id.localeCompare(a.id));
  }
  function priorityClass(value){return value.toLowerCase();}
  function statusClass(value){return value.toLowerCase().replaceAll(' ','-');}
  function render(){
    const list=matches();
    rows.innerHTML=list.map(report=>`<tr><td><div class="report-title-cell"><span class="report-type-icon ${report.type.toLowerCase()}">${report.type==='Task'?'T':'U'}</span><span><strong>${safe(report.reason)}</strong><small>${safe(report.id)} · ${report.type} report</small></span></div></td><td><div class="reported-item"><a href="${getHref(report)}">${safe(report.targetName)}</a><small>${safe(report.targetId)}</small></div></td><td><div class="reporter-cell"><span class="reporter-avatar">${safe(report.reporterInitials)}</span><span>${safe(report.reporter)}</span></div></td><td class="report-reason">${safe(report.reason)}</td><td>${safe(report.submitted)}</td><td><span class="priority-pill ${priorityClass(report.priority)}">${safe(report.priority)}</span></td><td><span class="report-status ${statusClass(report.status)}">${safe(report.status)}</span></td><td><button class="report-row-button" data-open-report="${safe(report.id)}">Review</button></td></tr>`).join('');
    document.getElementById('reportsEmpty').hidden=list.length>0;
    document.querySelector('.reports-table').hidden=list.length===0;
    document.getElementById('reportResult').textContent=`Showing ${list.length} sample report${list.length===1?'':'s'}`;
    document.getElementById('reportFooterCount').textContent=`${reports.length} sample reports`;
    document.getElementById('clearReportFilters').hidden=!(document.getElementById('reportSearch').value||activeStatus!=='all'||document.getElementById('typeFilter').value!=='all'||document.getElementById('reasonFilter').value!=='all'||document.getElementById('priorityFilter').value!=='all');
    const countStatus=status=>reports.filter(report=>report.status===status).length;
    const open=countStatus('Open');
    const review=countStatus('Under review');
    const resolved=countStatus('Resolved');
    const dismissed=countStatus('Dismissed');
    const high=reports.filter(report=>report.priority==='High'&&(report.status==='Open'||report.status==='Under review')).length;
    document.getElementById('openCount').textContent=open;
    document.getElementById('highCount').textContent=high;
    document.getElementById('reviewCount').textContent=review;
    document.getElementById('resolvedCount').textContent=resolved;
    document.getElementById('sidebarOpenCount').textContent=open;
    document.getElementById('sidebarOpenCount').hidden=open===0;
    document.getElementById('tabAll').textContent=reports.length;
    document.getElementById('tabOpen').textContent=open;
    document.getElementById('tabReview').textContent=review;
    document.getElementById('tabResolved').textContent=resolved;
    document.getElementById('tabDismissed').textContent=dismissed;
  }
  function closeModals(){document.querySelectorAll('.modal-backdrop').forEach(modal=>modal.hidden=true);pendingUpdate=null;}
  function actionButtons(report){
    if(report.status==='Open')return `<button class="button primary" data-outcome="Under review">Start review</button><button class="button secondary" data-outcome="Dismissed">Dismiss report</button>`;
    if(report.status==='Under review')return `<button class="button primary" data-outcome="Resolved">Resolve report</button><button class="button secondary" data-outcome="Dismissed">Dismiss report</button>`;
    return `<button class="button secondary" data-outcome="Open">Reopen report</button>`;
  }
  function showReport(id){
    const report=reports.find(item=>item.id===id);
    if(!report)return;
    const href=getHref(report);
    document.getElementById('reportModalContent').innerHTML=`<div class="report-modal-head"><span class="report-type-icon ${report.type.toLowerCase()}">${report.type==='Task'?'T':'U'}</span><span><h2 id="reportModalTitle">${safe(report.reason)}</h2><p>${safe(report.id)} · Submitted ${safe(report.submitted)}</p></span></div><div class="report-modal-badges"><span class="report-status ${statusClass(report.status)}">${safe(report.status)}</span><span class="priority-pill ${priorityClass(report.priority)}">${safe(report.priority)} priority</span><span class="category-chip">${report.type} report</span></div><div class="report-body">${safe(report.summary)}</div><div class="report-detail-grid"><div class="report-detail-item"><span>Reported item</span><a href="${href}">${safe(report.targetName)} · ${safe(report.targetId)}</a></div><div class="report-detail-item"><span>Submitted by</span><strong>${safe(report.reporter)}</strong></div><div class="report-detail-item"><span>Reason</span><strong>${safe(report.reason)}</strong></div><div class="report-detail-item"><span>Date received</span><strong>${safe(report.submitted)} · ${safe(report.date)}</strong></div><div class="report-detail-item"><span>Assigned reviewer</span><strong>${report.reviewer?safe(report.reviewer):'Unassigned'}</strong></div><div class="report-detail-item"><span>Report status</span><strong>${safe(report.status)}</strong></div></div><div class="report-evidence"><span>ⓘ</span><span>${safe(report.context)}</span></div>${report.note?`<div class="report-internal-note"><strong>INTERNAL DEMO NOTE</strong>${safe(report.note)}</div>`:''}<div class="report-modal-actions"><button class="button secondary" data-close-modal>Close</button><a class="button secondary" href="${href}">View reported ${report.type.toLowerCase()}</a>${actionButtons(report)}</div>`;
    document.getElementById('reportModal').hidden=false;
    document.querySelectorAll('#reportModalContent [data-close-modal]').forEach(button=>button.addEventListener('click',closeModals));
    document.querySelectorAll('#reportModalContent [data-outcome]').forEach(button=>button.addEventListener('click',()=>confirmOutcome(report.id,button.dataset.outcome)));
  }
  function confirmOutcome(id,status){
    const report=reports.find(item=>item.id===id);if(!report)return;
    closeModals();pendingUpdate={id,status};
    const labels={'Under review':'Start review','Resolved':'Resolve report','Dismissed':'Dismiss report','Open':'Reopen report'};
    document.getElementById('outcomeTitle').textContent=`${labels[status]}?`;
    document.getElementById('outcomeCopy').textContent=`${report.id} will be marked ${status.toLowerCase()} in this demo queue. No member will be contacted and no real moderation action will occur.`;
    document.getElementById('resolutionNote').value='';
    document.getElementById('resolutionField').hidden=status==='Under review'||status==='Open';
    const confirm=document.getElementById('confirmOutcome');
    confirm.textContent=labels[status];
    confirm.className=status==='Dismissed'?'button danger-button':'button primary';
    document.getElementById('outcomeModal').hidden=false;
  }
  function downloadCSV(){
    const rows=[['Report ID','Type','Reported item','Item ID','Reported by','Reason','Priority','Status','Submitted date'],...reports.map(report=>[report.id,report.type,report.targetName,report.targetId,report.reporter,report.reason,report.priority,report.status,report.date])];
    const csv=rows.map(row=>row.map(value=>`"${String(value).replaceAll('"','""')}"`).join(',')).join('\r\n');
    const url=URL.createObjectURL(new Blob([csv],{type:'text/csv;charset=utf-8'}));
    const anchor=document.createElement('a');anchor.href=url;anchor.download='tasktap-demo-reports.csv';document.body.appendChild(anchor);anchor.click();anchor.remove();URL.revokeObjectURL(url);notify('Sample report list exported as CSV.');
  }
  document.querySelectorAll('.future-link').forEach(link=>link.addEventListener('click',event=>{event.preventDefault();notify(`${link.dataset.page||'This page'} is planned for a future admin milestone.`);}));
  document.querySelectorAll('[data-toast]').forEach(link=>link.addEventListener('click',event=>{event.preventDefault();notify(link.dataset.toast);}));
  ['helpButton','notificationButton','profileButton','adminProfile'].forEach(id=>document.getElementById(id).addEventListener('click',()=>notify(id==='notificationButton'?'Report queue notifications are sample data.':'Admin Demo account controls are not connected.')));
  document.querySelectorAll('.report-tab').forEach(tab=>tab.addEventListener('click',()=>{activeStatus=tab.dataset.status;document.querySelectorAll('.report-tab').forEach(item=>{const selected=item===tab;item.classList.toggle('active',selected);item.setAttribute('aria-selected',selected?'true':'false');});render();}));
  ['reportSearch','typeFilter','reasonFilter','priorityFilter'].forEach(id=>document.getElementById(id).addEventListener(id==='reportSearch'?'input':'change',render));
  document.getElementById('clearReportFilters').addEventListener('click',()=>{document.getElementById('reportSearch').value='';document.getElementById('typeFilter').value='all';document.getElementById('reasonFilter').value='all';document.getElementById('priorityFilter').value='all';activeStatus='all';document.querySelectorAll('.report-tab').forEach(tab=>{const selected=tab.dataset.status==='all';tab.classList.toggle('active',selected);tab.setAttribute('aria-selected',selected?'true':'false');});render();});
  rows.addEventListener('click',event=>{const button=event.target.closest('[data-open-report]');if(button)showReport(button.dataset.openReport);});
  document.getElementById('exportReports').addEventListener('click',downloadCSV);
  document.getElementById('confirmOutcome').addEventListener('click',()=>{
    if(!pendingUpdate)return;
    const report=reports.find(item=>item.id===pendingUpdate.id);if(!report)return;
    const previous=report.status;report.status=pendingUpdate.status;
    if(report.status==='Under review')report.reviewer='Admin Demo';
    if(report.status==='Open')report.reviewer='';
    const note=document.getElementById('resolutionNote').value.trim();
    if(note)report.note=note;
    const id=report.id;const outcome=report.status;
    closeModals();render();notify(`${id} marked ${outcome.toLowerCase()} (demo).`);
  });
  document.querySelectorAll('[data-close-modal]').forEach(button=>button.addEventListener('click',closeModals));
  document.querySelectorAll('.modal-backdrop').forEach(modal=>modal.addEventListener('click',event=>{if(event.target===modal)closeModals();}));
  document.addEventListener('keydown',event=>{if(event.key==='Escape')closeModals();if((event.ctrlKey||event.metaKey)&&event.key.toLowerCase()==='k'){event.preventDefault();document.getElementById('reportSearch').focus();}});
  const sidebar=document.getElementById('sidebar');const scrim=document.getElementById('sidebarScrim');
  function closeSidebar(){sidebar.classList.remove('open');scrim.classList.remove('open');}
  document.getElementById('mobileMenu').addEventListener('click',()=>{sidebar.classList.toggle('open');scrim.classList.toggle('open');});scrim.addEventListener('click',closeSidebar);
  render();
  const requestedReport=new URLSearchParams(location.search).get('id');if(requestedReport)showReport(requestedReport);
})();
