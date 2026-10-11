(() => {
  const toast = document.getElementById('toast');
  const rows = document.getElementById('taskRows');
  const pageSize = 8;
  let toastTimer;
  let currentPage = 1;
  let activeTab = 'all';
  let pendingAction = null;
  const tasks = [
    {id:'TSK-1049',title:'Grocery pickup and delivery',owner:'Camille Santos',ownerId:'USR-2486',initials:'CS',category:'Shopping',area:'Brgy. 176',budget:450,posted:'2026-10-11',status:'Active',applicants:3,performer:'Not assigned',reported:false,description:'Pick up a short grocery list from the nearby market and deliver it to the task poster. The exact meeting point is shared after the task is accepted.'},
    {id:'TSK-1048',title:'Fix a leaking kitchen faucet',owner:'Trisha Bautista',ownerId:'USR-2484',initials:'TB',category:'Repairs',area:'Brgy. 179',budget:800,posted:'2026-10-11',status:'Active',applicants:4,performer:'Not assigned',reported:true,description:'Need help checking and repairing a small kitchen faucet leak. Please bring basic plumbing tools. Exact address is shared only after arrangements are confirmed.'},
    {id:'TSK-1046',title:'Math tutor for Grade 10',owner:'Bea Flores',ownerId:'USR-2480',initials:'BF',category:'Tutoring',area:'Brgy. 176',budget:600,posted:'2026-10-10',status:'In Progress',applicants:2,performer:'Mika Reyes',performerId:'USR-2478',reported:false,description:'Looking for a patient tutor for two algebra review sessions this week. Schedule and learning goals are arranged through the task conversation.'},
    {id:'TSK-1042',title:'Afternoon dog walking',owner:'Janelle Cruz',ownerId:'USR-2482',initials:'JC',category:'Pet Care',area:'Brgy. 178',budget:350,posted:'2026-10-10',status:'Active',applicants:5,performer:'Not assigned',reported:false,description:'Walk a friendly medium-sized dog for about 30 minutes in the neighborhood during the afternoon.'},
    {id:'TSK-1039',title:'One-bedroom apartment cleaning',owner:'Rafael Dizon',ownerId:'USR-2481',initials:'RD',category:'Cleaning',area:'Brgy. 179',budget:1200,posted:'2026-10-09',status:'Completed',applicants:3,performer:'Camille Santos',performerId:'USR-2486',reported:false,description:'General cleaning for a small apartment, including floors and kitchen surfaces. Cleaning supplies are available on site.'},
    {id:'TSK-1035',title:'Help move a small bookshelf',owner:'Mark Villanueva',ownerId:'USR-2483',initials:'MV',category:'Others',area:'Brgy. 176',budget:500,posted:'2026-10-09',status:'In Progress',applicants:2,performer:'Paolo Mendoza',performerId:'USR-2485',reported:false,description:'Need one person to help move a bookshelf between rooms and secure it in place.'},
    {id:'TSK-1032',title:'Garden cleanup and trimming',owner:'Anton Lim',ownerId:'USR-2477',initials:'AL',category:'Others',area:'Brgy. 176',budget:700,posted:'2026-10-08',status:'Paused',applicants:1,performer:'Not assigned',reported:false,description:'Trim a few shrubs and clear dry leaves from a small front garden. Task is paused while the poster updates the schedule.'},
    {id:'TSK-1028',title:'Install two wall shelves',owner:'Mika Reyes',ownerId:'USR-2478',initials:'MR',category:'Repairs',area:'Brgy. 179',budget:900,posted:'2026-10-07',status:'Completed',applicants:4,performer:'Mark Villanueva',performerId:'USR-2483',reported:false,description:'Install two small shelves on a concrete wall. Hardware is provided.'},
    {id:'TSK-1024',title:'Pick up prescription refill',owner:'Lara Domingo',ownerId:'USR-2476',initials:'LD',category:'Delivery',area:'Brgy. 178',budget:250,posted:'2026-10-06',status:'Active',applicants:2,performer:'Not assigned',reported:false,description:'Pick up a prepared pharmacy refill and bring it to the task poster. Private details are exchanged after acceptance.'},
    {id:'TSK-1019',title:'Help set up a laptop',owner:'Noel Garcia',ownerId:'USR-2479',initials:'NG',category:'Others',area:'Brgy. 178',budget:550,posted:'2026-10-05',status:'Archived',applicants:0,performer:'Not assigned',reported:false,description:'Basic setup help for a new laptop, including Wi-Fi and printer connection.'},
    {id:'TSK-1015',title:'Deliver lunch to a neighbor',owner:'Enzo Ramirez',ownerId:'USR-2475',initials:'ER',category:'Delivery',area:'Brgy. 179',budget:300,posted:'2026-10-04',status:'Cancelled',applicants:1,performer:'Not assigned',reported:true,description:'A meal delivery task that was cancelled by the poster. A report is attached for sample moderation context.'},
    {id:'TSK-1011',title:'Water plants while away',owner:'Camille Santos',ownerId:'USR-2486',initials:'CS',category:'Others',area:'Brgy. 176',budget:400,posted:'2026-10-03',status:'Completed',applicants:2,performer:'Janelle Cruz',performerId:'USR-2482',reported:false,description:'Water indoor plants twice during the weekend. Access and timing were arranged privately by the members.'}
  ];
  function notify(message) {
    toast.textContent = message;
    toast.classList.add('show');
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => toast.classList.remove('show'), 2800);
  }
  function safe(value) {
    return String(value).replace(/[&<>"']/g, character => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[character]));
  }
  function prettyDate(value) { return new Date(`${value}T00:00:00`).toLocaleDateString('en-PH',{month:'short',day:'numeric',year:'numeric'}); }
  function iconFor(category) {
    if (category === 'Repairs') return ['⌂','kind-home'];
    if (category === 'Shopping' || category === 'Delivery') return ['➤','kind-errand'];
    if (category === 'Tutoring') return ['✎','kind-learning'];
    if (category === 'Pet Care') return ['♡','kind-care'];
    if (category === 'Cleaning') return ['✧','kind-clean'];
    if (category === 'Gardening') return ['❀','kind-garden'];
    return ['▤','kind-home'];
  }
  function filteredTasks() {
    const query = document.getElementById('taskSearch').value.trim().toLowerCase();
    const category = document.getElementById('categoryFilter').value;
    const barangay = document.getElementById('barangayFilter').value;
    const sort = document.getElementById('sortTasks').value;
    let matches = tasks.filter(task => {
      const searchMatches = !query || `${task.title} ${task.id} ${task.owner} ${task.ownerId} ${task.performer || ''} ${task.performerId || ''}`.toLowerCase().includes(query);
      const tabMatches = activeTab === 'all' || (activeTab === 'reported' ? task.reported : task.status === activeTab);
      return searchMatches && tabMatches && (category === 'all' || task.category === category) && (barangay === 'all' || task.area === barangay);
    });
    if (sort === 'oldest') matches.sort((a,b) => a.posted.localeCompare(b.posted));
    if (sort === 'recent') matches.sort((a,b) => b.posted.localeCompare(a.posted));
    if (sort === 'budget-high') matches.sort((a,b) => b.budget-a.budget);
    if (sort === 'budget-low') matches.sort((a,b) => a.budget-b.budget);
    return matches;
  }
  function render() {
    const matches = filteredTasks();
    const totalPages = Math.max(1,Math.ceil(matches.length/pageSize));
    currentPage = Math.min(currentPage,totalPages);
    const start = (currentPage-1)*pageSize;
    const visible = matches.slice(start,start+pageSize);
    rows.innerHTML = visible.map(task => {
      const [icon,iconClass] = iconFor(task.category);
      const statusClass = task.status.toLowerCase().replaceAll(' ','-');
      return `<tr data-id="${safe(task.id)}"><td class="task-check"><input class="task-row-check" type="checkbox" aria-label="Select ${safe(task.title)}"></td><td><div class="task-name-cell"><span class="task-kind-icon ${iconClass}">${icon}</span><span><strong>${safe(task.title)}</strong><small>${safe(task.id)}${task.reported ? '<span class="flag-pill">⚑ Reported</span>' : ''}</small></span></div></td><td><div class="poster-cell"><span class="poster-avatar">${safe(task.initials)}</span><span>${safe(task.owner)}</span></div></td><td><span class="category-chip">${safe(task.category)}</span></td><td>${safe(task.area)}</td><td class="task-budget">₱${task.budget.toLocaleString()}<small>Task budget</small></td><td>${prettyDate(task.posted)}</td><td><span class="task-status ${statusClass}">${safe(task.status)}</span></td><td><button class="task-more" data-view="${safe(task.id)}" aria-label="View ${safe(task.title)}">···</button></td></tr>`;
    }).join('');
    document.getElementById('taskEmpty').hidden = matches.length > 0;
    document.querySelector('.task-table').hidden = matches.length === 0;
    document.getElementById('taskResultSummary').textContent = matches.length ? `Showing ${start+1}–${Math.min(start+pageSize,matches.length)} of ${matches.length} sample tasks` : 'No sample tasks match these filters';
    document.getElementById('taskPageLabel').textContent = `Page ${currentPage} of ${totalPages}`;
    document.getElementById('currentTaskPage').textContent = currentPage;
    document.getElementById('prevTaskPage').disabled = currentPage <= 1;
    document.getElementById('nextTaskPage').disabled = currentPage >= totalPages;
    document.getElementById('selectAllTasks').checked = false;
    updateSelected();
    document.getElementById('clearTaskFilters').hidden = !(document.getElementById('taskSearch').value || activeTab !== 'all' || document.getElementById('categoryFilter').value !== 'all' || document.getElementById('barangayFilter').value !== 'all');
    updateTabCounts();
  }
  function updateSelected() {
    const count = rows.querySelectorAll('.task-row-check:checked').length;
    const info = document.getElementById('selectedTaskInfo');
    info.textContent = count ? `${count} task${count===1?'':'s'} selected` : 'Select rows to export selected tasks.';
    info.classList.toggle('selected',count>0);
    document.getElementById('exportSelectedTasks').hidden = count === 0;
  }
  function updateTabCounts() {
    document.getElementById('tab-all').textContent = tasks.length;
    document.getElementById('tab-active').textContent = tasks.filter(task=>task.status==='Active').length;
    document.getElementById('tab-progress').textContent = tasks.filter(task=>task.status==='In Progress').length;
    document.getElementById('tab-completed').textContent = tasks.filter(task=>task.status==='Completed').length;
    document.getElementById('tab-reported').textContent = tasks.filter(task=>task.reported).length;
    document.getElementById('tab-archived').textContent = tasks.filter(task=>task.status==='Archived').length;
  }
  function showModal(id) { document.getElementById(id).hidden = false; }
  function closeModals() { document.querySelectorAll('.modal-backdrop').forEach(modal=>{modal.hidden=true;}); pendingAction=null; }
  function viewTask(id) {
    const task = tasks.find(item=>item.id===id);
    if (!task) return;
    const [icon,iconClass] = iconFor(task.category);
    const statusClass = task.status.toLowerCase().replaceAll(' ','-');
    const performer = task.performer === 'Not assigned' ? 'Not assigned' : `<a class="task-member-link" href="[Admin] user-detail.html?id=${encodeURIComponent(task.performerId)}">${safe(task.performer)}</a>`;
    document.getElementById('taskModalContent').innerHTML = `<div class="task-modal-head"><span class="task-kind-icon ${iconClass}">${icon}</span><span><h2 id="taskModalTitle">${safe(task.title)}</h2><p>${safe(task.id)} · Posted ${prettyDate(task.posted)}</p></span></div><div class="task-modal-badges"><span class="category-chip">${safe(task.category)}</span><span class="task-status ${statusClass}">${safe(task.status)}</span>${task.reported?'<span class="flag-pill">⚑ Reported</span>':''}</div><p class="task-description">${safe(task.description)}</p><div class="task-detail-grid"><div class="task-detail-item"><span>Posted by</span><strong><a class="task-member-link" href="[Admin] user-detail.html?id=${encodeURIComponent(task.ownerId)}">${safe(task.owner)}</a></strong></div><div class="task-detail-item"><span>Performer</span><strong>${performer}</strong></div><div class="task-detail-item"><span>Approximate area</span><strong>${safe(task.area)}, Caloocan City</strong></div><div class="task-detail-item"><span>Task budget</span><strong>₱${task.budget.toLocaleString()}</strong></div><div class="task-detail-item"><span>Requests</span><strong>${task.applicants}</strong></div><div class="task-detail-item"><span>Task status</span><strong>${safe(task.status)}</strong></div></div><div class="task-safety-note"><span>ⓘ</span><span>Location is shown at barangay level. The amount is a member-posted task budget; TaskTap does not process payments.</span></div><div class="modal-actions"><button class="button secondary" data-close-modal>Close</button>${task.reported?'<button class="button secondary" id="reviewReportAction">Mark report reviewed</button>':''}<button class="button ${task.status==='Archived'?'primary':'danger-button'}" id="taskAction">${actionLabel(task)}</button></div>`;
    showModal('taskModal');
    document.querySelectorAll('#taskModalContent [data-close-modal]').forEach(button=>button.addEventListener('click',closeModals));
    document.getElementById('taskAction').addEventListener('click',()=>confirmTaskChange(task.id,'status'));
    const reportButton=document.getElementById('reviewReportAction');
    if(reportButton) reportButton.addEventListener('click',()=>confirmTaskChange(task.id,'report'));
  }
  function actionLabel(task) {
    if (task.status==='Archived') return 'Restore task';
    if (task.status==='Completed'||task.status==='Cancelled') return 'Archive record';
    if (task.status==='Paused') return 'Resume task';
    return 'Pause task';
  }
  function confirmTaskChange(id,kind) {
    const task=tasks.find(item=>item.id===id);
    if(!task)return;
    pendingAction={id,kind};
    const action = kind==='report'?'Mark this report reviewed':actionLabel(task);
    document.getElementById('confirmTitle').textContent=`${action}?`;
    document.getElementById('confirmCopy').textContent=kind==='report'?'This clears the report flag from this sample listing only. No moderation system is connected.':`This updates the sample status for “${task.title}” on this page only. No real listing will change.`;
    const button=document.getElementById('confirmTaskAction');
    button.textContent=action;
    button.className=kind==='report'||task.status==='Archived'?'button primary':'button danger-button';
    closeModals();
    pendingAction={id,kind};
    showModal('confirmModal');
  }
  function downloadCSV(list,filename) {
    const header=['Task ID','Task title','Posted by','Category','Barangay','Task budget (PHP)','Posted date','Status','Reported'];
    const lines=[header,...list.map(task=>[task.id,task.title,task.owner,task.category,task.area,task.budget,task.posted,task.status,task.reported?'Yes':'No'])];
    const csv=lines.map(line=>line.map(value=>`"${String(value).replaceAll('"','""')}"`).join(',')).join('\r\n');
    const url=URL.createObjectURL(new Blob([csv],{type:'text/csv;charset=utf-8'}));
    const anchor=document.createElement('a');anchor.href=url;anchor.download=filename;document.body.appendChild(anchor);anchor.click();anchor.remove();URL.revokeObjectURL(url);
    notify(`${list.length} sample task record${list.length===1?'':'s'} exported.`);
  }

  document.querySelectorAll('.future-link').forEach(link=>link.addEventListener('click',event=>{event.preventDefault();notify(`${link.dataset.page||'This page'} is planned for a future admin milestone.`);}));
  document.querySelectorAll('[data-toast]').forEach(link=>link.addEventListener('click',event=>{event.preventDefault();notify(link.dataset.toast);}));
  ['helpButton','notificationButton','profileButton','adminProfile'].forEach(id=>document.getElementById(id).addEventListener('click',()=>notify(id==='notificationButton'?'No new task alerts in this demo.':'Admin Demo account controls are not connected.')));
  document.querySelectorAll('[data-close-modal]').forEach(button=>button.addEventListener('click',closeModals));
  document.querySelectorAll('.modal-backdrop').forEach(modal=>modal.addEventListener('click',event=>{if(event.target===modal)closeModals();}));
  document.addEventListener('keydown',event=>{if(event.key==='Escape')closeModals();if((event.ctrlKey||event.metaKey)&&event.key.toLowerCase()==='k'){event.preventDefault();document.getElementById('taskSearch').focus();}});
  document.querySelectorAll('.task-tab').forEach(tab=>tab.addEventListener('click',()=>{activeTab=tab.dataset.status;currentPage=1;document.querySelectorAll('.task-tab').forEach(item=>{const selected=item===tab;item.classList.toggle('active',selected);item.setAttribute('aria-selected',selected?'true':'false');});render();}));
  ['taskSearch','categoryFilter','barangayFilter','sortTasks'].forEach(id=>document.getElementById(id).addEventListener(id==='taskSearch'?'input':'change',()=>{currentPage=1;render();}));
  document.getElementById('clearTaskFilters').addEventListener('click',()=>{document.getElementById('taskSearch').value='';document.getElementById('categoryFilter').value='all';document.getElementById('barangayFilter').value='all';activeTab='all';currentPage=1;document.querySelectorAll('.task-tab').forEach(tab=>{const selected=tab.dataset.status==='all';tab.classList.toggle('active',selected);tab.setAttribute('aria-selected',selected?'true':'false');});render();});
  document.getElementById('prevTaskPage').addEventListener('click',()=>{currentPage--;render();});
  document.getElementById('nextTaskPage').addEventListener('click',()=>{currentPage++;render();});
  document.getElementById('selectAllTasks').addEventListener('change',event=>{rows.querySelectorAll('.task-row-check').forEach(input=>input.checked=event.target.checked);updateSelected();});
  rows.addEventListener('change',event=>{if(event.target.matches('.task-row-check')){const checks=[...rows.querySelectorAll('.task-row-check')];document.getElementById('selectAllTasks').checked=checks.length>0&&checks.every(input=>input.checked);updateSelected();}});
  rows.addEventListener('click',event=>{const button=event.target.closest('[data-view]');if(button)viewTask(button.dataset.view);});
  document.getElementById('exportButton').addEventListener('click',()=>downloadCSV(tasks,'tasktap-demo-tasks.csv'));
  document.getElementById('exportSelectedTasks').addEventListener('click',()=>{const ids=[...rows.querySelectorAll('.task-row-check:checked')].map(input=>input.closest('tr').dataset.id);downloadCSV(tasks.filter(task=>ids.includes(task.id)),'tasktap-demo-selected-tasks.csv');});
  document.getElementById('confirmTaskAction').addEventListener('click',()=>{
    if(!pendingAction)return;
    const task=tasks.find(item=>item.id===pendingAction.id);
    if(!task)return;
    if(pendingAction.kind==='report')task.reported=false;
    else if(task.status==='Archived')task.status='Active';
    else if(task.status==='Completed'||task.status==='Cancelled')task.status='Archived';
    else if(task.status==='Paused')task.status='Active';
    else task.status='Paused';
    const taskTitle=task.title;
    closeModals();render();notify(`“${taskTitle}” updated in this demo view.`);
  });
  const sidebar=document.getElementById('sidebar');
  const scrim=document.getElementById('sidebarScrim');
  function closeSidebar(){sidebar.classList.remove('open');scrim.classList.remove('open');}
  document.getElementById('mobileMenu').addEventListener('click',()=>{sidebar.classList.toggle('open');scrim.classList.toggle('open');});
  scrim.addEventListener('click',closeSidebar);
  const linkedTask=new URLSearchParams(location.search).get('task');
  if(linkedTask)document.getElementById('taskSearch').value=linkedTask;
  render();
})();
