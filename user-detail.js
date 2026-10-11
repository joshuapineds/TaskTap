(() => {
  const toast = document.getElementById('toast');
  let toastTimer;
  const storedSelection = (() => {
    try {
      const raw = sessionStorage.getItem('tasktapAdminSelectedMember');
      const record = raw ? JSON.parse(raw) : null;
      return record && record.id === new URLSearchParams(location.search).get('id') ? record : null;
    } catch { return null; }
  })();
  const sampleUsers = {
    'USR-2486': {id:'USR-2486',name:'Camille Santos',email:'camille.santos@example.com',initials:'CS',color:'peach',role:'Both',barangay:'Brgy. 176',joined:'2026-10-11',tasks:2,completed:2,verification:'Verified',status:'Active',phone:'+63 917 ••• 0184',reports:0,reported:0,rating:'4.9',ratingCount:12,positive:'98%',lastActive:'Today, 6:42 PM',emailVerified:true,phoneVerified:true,identity:'Not requested'},
    'USR-2485': {id:'USR-2485',name:'Paolo Mendoza',email:'paolo.mendoza@example.com',initials:'PM',color:'mint',role:'Task performer',barangay:'Brgy. 178',joined:'2026-10-10',tasks:0,completed:0,verification:'Pending',status:'Pending',phone:'+63 918 ••• 2640',reports:0,reported:0,rating:'—',ratingCount:0,positive:'—',lastActive:'Today, 4:16 PM',emailVerified:true,phoneVerified:false,identity:'Not requested'},
    'USR-2484': {id:'USR-2484',name:'Trisha Bautista',email:'trisha.bautista@example.com',initials:'TB',color:'violet',role:'Task poster',barangay:'Brgy. 179',joined:'2026-10-09',tasks:5,completed:4,verification:'Verified',status:'Active',phone:'+63 905 ••• 3381',reports:0,reported:0,rating:'4.8',ratingCount:7,positive:'96%',lastActive:'Today, 2:10 PM',emailVerified:true,phoneVerified:true,identity:'Not requested'},
    'USR-2483': {id:'USR-2483',name:'Mark Villanueva',email:'mark.villanueva@example.com',initials:'MV',color:'blue',role:'Both',barangay:'Brgy. 176',joined:'2026-10-08',tasks:11,completed:9,verification:'Verified',status:'Active',phone:'+63 917 ••• 5502',reports:0,reported:0,rating:'5.0',ratingCount:15,positive:'100%',lastActive:'Yesterday, 8:35 PM',emailVerified:true,phoneVerified:true,identity:'Verified'},
    'USR-2482': {id:'USR-2482',name:'Janelle Cruz',email:'janelle.cruz@example.com',initials:'JC',color:'rose',role:'Task performer',barangay:'Brgy. 178',joined:'2026-10-07',tasks:3,completed:2,verification:'Unverified',status:'Active',phone:'+63 926 ••• 7190',reports:0,reported:0,rating:'4.7',ratingCount:3,positive:'94%',lastActive:'Oct 10, 11:20 AM',emailVerified:false,phoneVerified:false,identity:'Not requested'},
    'USR-2481': {id:'USR-2481',name:'Rafael Dizon',email:'rafael.dizon@example.com',initials:'RD',color:'gold',role:'Task poster',barangay:'Brgy. 179',joined:'2026-10-06',tasks:6,completed:1,verification:'Pending',status:'Pending',phone:'+63 917 ••• 6285',reports:1,reported:0,rating:'4.2',ratingCount:2,positive:'82%',lastActive:'Today, 12:05 PM',emailVerified:true,phoneVerified:false,identity:'Not requested'},
    'USR-2480': {id:'USR-2480',name:'Bea Flores',email:'bea.flores@example.com',initials:'BF',color:'sky',role:'Both',barangay:'Brgy. 176',joined:'2026-10-05',tasks:8,completed:7,verification:'Verified',status:'Active',phone:'+63 908 ••• 1476',reports:0,reported:0,rating:'4.9',ratingCount:9,positive:'99%',lastActive:'Today, 9:42 AM',emailVerified:true,phoneVerified:true,identity:'Verified'},
    'USR-2479': {id:'USR-2479',name:'Noel Garcia',email:'noel.garcia@example.com',initials:'NG',color:'lilac',role:'Task performer',barangay:'Brgy. 178',joined:'2026-10-04',tasks:1,completed:0,verification:'Unverified',status:'Suspended',phone:'+63 919 ••• 4207',reports:2,reported:0,rating:'—',ratingCount:0,positive:'—',lastActive:'Oct 8, 6:18 PM',emailVerified:false,phoneVerified:false,identity:'Not requested'},
    'USR-2478': {id:'USR-2478',name:'Mika Reyes',email:'mika.reyes@example.com',initials:'MR',color:'mint',role:'Both',barangay:'Brgy. 179',joined:'2026-10-03',tasks:9,completed:8,verification:'Verified',status:'Active',phone:'+63 917 ••• 2931',reports:0,reported:0,rating:'4.9',ratingCount:11,positive:'99%',lastActive:'Today, 1:03 PM',emailVerified:true,phoneVerified:true,identity:'Verified'},
    'USR-2477': {id:'USR-2477',name:'Anton Lim',email:'anton.lim@example.com',initials:'AL',color:'peach',role:'Task poster',barangay:'Brgy. 176',joined:'2026-10-02',tasks:4,completed:3,verification:'Verified',status:'Active',phone:'+63 915 ••• 7012',reports:0,reported:0,rating:'4.8',ratingCount:5,positive:'97%',lastActive:'Oct 10, 7:55 PM',emailVerified:true,phoneVerified:true,identity:'Not requested'},
    'USR-2476': {id:'USR-2476',name:'Lara Domingo',email:'lara.domingo@example.com',initials:'LD',color:'blue',role:'Task performer',barangay:'Brgy. 178',joined:'2026-10-01',tasks:0,completed:0,verification:'Pending',status:'Pending',phone:'+63 916 ••• 8330',reports:0,reported:0,rating:'—',ratingCount:0,positive:'—',lastActive:'Oct 9, 9:15 AM',emailVerified:true,phoneVerified:false,identity:'Not requested'},
    'USR-2475': {id:'USR-2475',name:'Enzo Ramirez',email:'enzo.ramirez@example.com',initials:'ER',color:'violet',role:'Both',barangay:'Brgy. 179',joined:'2026-09-30',tasks:12,completed:10,verification:'Verified',status:'Suspended',phone:'+63 927 ••• 3415',reports:3,reported:1,rating:'3.9',ratingCount:8,positive:'72%',lastActive:'Oct 7, 10:12 PM',emailVerified:true,phoneVerified:true,identity:'Verified'}
  };
  const requestedId = new URLSearchParams(location.search).get('id') || 'USR-2486';
  const user = storedSelection || sampleUsers[requestedId] || null;
  const fallback = {id:requestedId,name:'Member not found',email:'No matching sample profile',initials:'?',color:'blue',role:'Both',barangay:'Brgy. 176',joined:'2026-10-11',tasks:0,completed:0,verification:'Unverified',status:'Active',phone:'Not provided',reports:0,reported:0,rating:'—',ratingCount:0,positive:'—',lastActive:'No activity recorded',emailVerified:false,phoneVerified:false,identity:'Not requested'};
  const member = user || fallback;
  const profileEvents = {
    'USR-2486': [
      {icon:'task',symbol:'▤',title:'Posted a new task',text:'“Grocery pickup and delivery” · Brgy. 176',when:'Today, 6:42 PM'},
      {icon:'complete',symbol:'✓',title:'Task marked completed',text:'“Help with moving boxes” · Payment record added by member',when:'Oct 9, 2026'},
      {icon:'review',symbol:'★',title:'Received a 5-star review',text:'“Very kind and easy to coordinate with.”',when:'Oct 9, 2026'},
      {icon:'verify',symbol:'✓',title:'Mobile number verified',text:'Contact details confirmed in the sample profile.',when:'Oct 8, 2026'},
      {icon:'join',symbol:'♙',title:'Joined TaskTap',text:'Member account created.',when:'Oct 11, 2026'}
    ],
    'USR-2484': [
      {icon:'task',symbol:'▤',title:'Posted a new task',text:'“Help assemble a study desk” · Brgy. 179',when:'Today, 2:10 PM'},
      {icon:'complete',symbol:'✓',title:'Task marked completed',text:'“Window screen repair” · Completed with a member review.',when:'Oct 10, 2026'},
      {icon:'verify',symbol:'✓',title:'Email address verified',text:'Account contact verified.',when:'Oct 9, 2026'},
      {icon:'join',symbol:'♙',title:'Joined TaskTap',text:'Member account created.',when:'Oct 9, 2026'}
    ],
    'USR-2481': [
      {icon:'report',symbol:'⚑',title:'A report was submitted',text:'Sample report for review · no action taken in this prototype.',when:'Today, 12:05 PM'},
      {icon:'task',symbol:'▤',title:'Updated a task post',text:'“Small home repair help” · Brgy. 179',when:'Oct 8, 2026'},
      {icon:'join',symbol:'♙',title:'Joined TaskTap',text:'Member account created.',when:'Oct 6, 2026'}
    ]
  };
  const events = profileEvents[member.id] || (member.id === requestedId && !sampleUsers[requestedId] && !storedSelection ? [] : [
    {icon:'task',symbol:'▤',title:'Member activity recorded',text:`${member.tasks} tasks are associated with this sample account.`,when:member.lastActive || 'Recently'},
    {icon:member.verification === 'Verified' ? 'verify' : 'join',symbol:member.verification === 'Verified' ? '✓' : '♙',title:member.verification === 'Verified' ? 'Account verification complete' : 'Joined TaskTap',text:'Sample member account event.',when:member.joined}
  ]);
  function showToast(message) {
    toast.textContent = message;
    toast.classList.add('show');
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => toast.classList.remove('show'), 2800);
  }
  function prettyDate(value, options = {month:'short',day:'numeric',year:'numeric'}) {
    return new Date(`${value}T00:00:00`).toLocaleDateString('en-PH', options);
  }
  function roleText(role) { return role === 'Both' ? 'Task poster and performer' : role; }
  function setPill(element, label, className) { element.textContent = label; element.className = className; }
  function applyMember() {
    if (!user) {
      document.querySelector('.detail-demo-tag').textContent = 'SAMPLE PROFILE NOT FOUND';
      document.getElementById('memberName').textContent = 'Member not found';
      document.querySelector('.hero-meta').innerHTML = `No sample record matches ${requestedId}. <a href="[Admin] user-management.html">Return to member list</a>`;
      document.getElementById('statusAction').disabled = true;
    }
    const joined = member.joined ? prettyDate(member.joined) : '—';
    document.title = `TaskTap Admin — ${member.name}`;
    document.getElementById('heroAvatar').textContent = member.initials || member.name.split(/\s+/).slice(0,2).map(part => part[0]).join('').toUpperCase();
    document.getElementById('heroAvatar').className = `hero-avatar ${member.color || 'blue'}`;
    document.getElementById('memberName').textContent = member.name;
    document.getElementById('memberIdTop').textContent = member.id;
    document.getElementById('memberEmailTop').textContent = member.email;
    document.getElementById('memberJoinedTop').textContent = joined;
    const roleClass = member.role === 'Both' ? 'both-role' : member.role === 'Task poster' ? 'poster-role' : 'performer-role';
    setPill(document.getElementById('memberRole'), member.role, `role-pill ${roleClass}`);
    setPill(document.getElementById('memberVerification'), member.verification, `verification-pill ${member.verification.toLowerCase()}`);
    updateStatusPill();
    document.getElementById('infoName').textContent = member.name;
    document.getElementById('infoEmail').textContent = member.email;
    document.getElementById('infoPhone').textContent = member.phone || 'Not provided';
    document.getElementById('infoBarangay').textContent = `${member.barangay || 'Not provided'}, Caloocan City`;
    document.getElementById('infoId').textContent = member.id;
    document.getElementById('infoJoined').textContent = member.joined ? prettyDate(member.joined,{month:'long',day:'numeric',year:'numeric'}) : '—';
    document.getElementById('infoRole').textContent = roleText(member.role);
    document.getElementById('lastActive').textContent = member.lastActive || 'Today, 6:42 PM';
    document.getElementById('tasksPosted').textContent = member.tasks ?? 0;
    document.getElementById('tasksCompleted').textContent = member.completed ?? 0;
    document.getElementById('memberRating').innerHTML = `${member.rating || '—'} <small>${member.rating && member.rating !== '—' ? '/ 5' : ''}</small>`;
    document.getElementById('ratingCount').textContent = member.ratingCount ? `Based on ${member.ratingCount} reviews` : 'No reviews yet';
    document.getElementById('reportsReceived').textContent = member.reports ?? 0;
    document.getElementById('reportFootnote').textContent = member.reports ? `${member.reports} report${member.reports === 1 ? '' : 's'} in sample data` : 'No open concerns';
    document.getElementById('taskTabCount').textContent = member.tasks ?? 0;
    document.getElementById('reviewTabCount').textContent = member.ratingCount ?? 0;
    document.getElementById('reportTabCount').textContent = member.reports ?? 0;
    document.getElementById('positiveRate').textContent = member.positive || '—';
    document.getElementById('memberReportsFiled').textContent = member.reported ?? 0;
    document.getElementById('standingLabel').textContent = member.status === 'Suspended' ? 'Suspended' : member.reports ? 'Review context' : 'Good standing';
    document.getElementById('standingLabel').className = member.status === 'Suspended' ? 'standing-bad' : member.reports ? 'standing-review' : 'standing-good';
    const emailVerified = Boolean(member.emailVerified ?? member.verification === 'Verified');
    const phoneVerified = Boolean(member.phoneVerified ?? member.verification === 'Verified');
    document.getElementById('emailVerified').textContent = emailVerified ? '✓ Verified' : 'Not verified';
    document.getElementById('emailVerified').className = emailVerified ? 'inline-verified' : 'inline-pending';
    document.getElementById('emailVerificationText').textContent = emailVerified ? 'Verified' : 'Not verified';
    document.getElementById('phoneVerificationText').textContent = phoneVerified ? 'Confirmed' : 'Not confirmed';
    document.getElementById('phoneCheck').className = phoneVerified ? 'verification-check' : 'verification-pending';
    document.getElementById('phoneCheck').textContent = phoneVerified ? '✓' : '○';
    document.getElementById('phoneVerifiedLabel').textContent = phoneVerified ? 'Verified' : 'Pending';
    document.getElementById('identityVerificationText').textContent = member.identity || 'Not requested';
    document.getElementById('identityCheck').className = member.identity === 'Verified' ? 'verification-check' : 'verification-pending';
    document.getElementById('identityCheck').textContent = member.identity === 'Verified' ? '✓' : '○';
    document.getElementById('identityVerifiedLabel').textContent = member.identity === 'Verified' ? 'Verified' : 'Optional';
    renderEvents(); renderTasks(); renderReviews(); renderReports();
  }
  function updateStatusPill() {
    const status = document.getElementById('memberStatus');
    const statusClass = member.status === 'Suspended' ? 'suspended-status' : member.status === 'Pending' ? 'pending-status' : 'active-status';
    setPill(status, member.status, `status-pill ${statusClass}`);
    const action = document.getElementById('statusAction');
    action.textContent = member.status === 'Suspended' ? 'Reactivate account' : 'Suspend account';
    action.className = member.status === 'Suspended' ? 'button primary' : 'button danger-button';
  }
  function eventMarkup(item) {
    return `<div class="timeline-entry"><span class="timeline-icon ${item.icon}">${item.symbol}</span><div class="timeline-copy"><strong>${item.title}</strong><p>${item.text}</p></div><time>${item.when}</time></div>`;
  }
  function renderEvents() {
    const html = events.map(eventMarkup).join('');
    document.getElementById('overviewTimeline').innerHTML = html || '<div class="empty-tab"><span>◷</span><strong>No activity recorded</strong><p>This sample profile has no activity events.</p></div>';
    document.getElementById('fullTimeline').innerHTML = html || '<div class="empty-tab"><span>◷</span><strong>No activity recorded</strong><p>This sample profile has no activity events.</p></div>';
  }
  function renderTasks() {
    const taskExamples = member.id === 'USR-2486' ? [
      {name:'Grocery pickup and delivery',meta:'Posted · Brgy. 176 · TSK-1049',category:'Errands',status:'Active',date:'Today'},
      {name:'Help with moving boxes',meta:'Performed · Brgy. 178 · TSK-0981',category:'Moving help',status:'Completed',date:'Oct 9, 2026'}
    ] : member.id === 'USR-2484' ? [
      {name:'Help assemble a study desk',meta:'Posted · Brgy. 179 · TSK-1046',category:'Home & Repairs',status:'Active',date:'Today'},
      {name:'Window screen repair',meta:'Posted · Brgy. 179 · TSK-1002',category:'Home & Repairs',status:'Completed',date:'Oct 10, 2026'}
    ] : [];
    const html = taskExamples.map(task => `<div class="task-row"><div class="task-title"><strong>${task.name}</strong><small>${task.meta}</small></div><span class="task-category">${task.category}</span><span class="task-state ${task.status === 'Active' ? 'in-progress' : ''}">${task.status}</span><span class="task-date">${task.date}</span></div>`).join('');
    document.getElementById('taskHistory').innerHTML = html || '<div class="empty-tab"><span>▤</span><strong>No task sample available</strong><p>Task history will appear here when records are connected.</p></div>';
  }
  function renderReviews() {
    const reviews = member.id === 'USR-2486' ? [
      {initials:'JR',name:'Jose R.',stars:5,text:'Very kind and easy to coordinate with. The task was completed on time.',date:'Oct 9, 2026'},
      {initials:'MT',name:'Mia T.',stars:5,text:'Clear communication and a smooth handoff. Would work together again.',date:'Oct 3, 2026'},
      {initials:'AL',name:'Anton L.',stars:4,text:'Helpful and reliable. Thank you for the support!',date:'Sep 28, 2026'}
    ] : member.id === 'USR-2484' ? [{initials:'BF',name:'Bea F.',stars:5,text:'The task details were clear and the member was friendly.',date:'Oct 10, 2026'}] : [];
    const html = reviews.map(review => `<article class="review-card"><span class="review-avatar">${review.initials}</span><div class="review-copy"><strong>${review.name}</strong><span class="review-stars">${'★'.repeat(review.stars)}${'☆'.repeat(5-review.stars)}</span><p>${review.text}</p><small>Sample review · ${review.date}</small></div><time>${review.date}</time></article>`).join('');
    document.getElementById('reviewHistory').innerHTML = html || '<div class="empty-tab"><span>★</span><strong>No sample reviews</strong><p>There are no review excerpts available for this profile.</p></div>';
  }
  function renderReports() {
    const reports = member.reports ? [{title:'Account concern reported',text:'Sample moderation context attached to this account. Review the related report before taking action.',date:'Oct 10, 2026',state:member.status === 'Suspended' ? 'Closed' : 'Needs review'}] : [];
    const html = reports.map(report => `<article class="report-card"><span class="report-symbol">⚑</span><div class="report-copy"><strong>${report.title}</strong><p>${report.text}</p><small>Sample report · ${report.date}</small></div><span class="report-state">${report.state}</span></article>`).join('');
    document.getElementById('reportsHistory').innerHTML = html || '<div class="empty-tab"><span>✓</span><strong>No reports in this sample</strong><p>This profile has no reports attached in the demo data.</p></div>';
  }
  function switchTab(tabName) {
    document.querySelectorAll('.detail-tab').forEach(tab => { const selected = tab.dataset.tab === tabName; tab.classList.toggle('active', selected); tab.setAttribute('aria-selected', selected ? 'true' : 'false'); });
    document.querySelectorAll('.tab-content').forEach(panel => { panel.hidden = panel.id !== `${tabName}Panel`; });
  }
  document.querySelectorAll('.detail-tab').forEach(tab => tab.addEventListener('click', () => switchTab(tab.dataset.tab)));
  document.querySelectorAll('[data-switch-tab]').forEach(button => button.addEventListener('click', () => switchTab(button.dataset.switchTab)));
  document.querySelectorAll('.future-link').forEach(link => link.addEventListener('click', event => { event.preventDefault(); showToast(`${link.dataset.page || 'This page'} is planned for a future admin milestone.`); }));
  document.querySelectorAll('[data-toast]').forEach(link => link.addEventListener('click', event => { event.preventDefault(); showToast(link.dataset.toast); }));
  ['helpButton','notificationButton','profileButton','adminProfile','moreActions'].forEach(id => document.getElementById(id).addEventListener('click', () => showToast(id === 'notificationButton' ? 'No new member alerts in this demo.' : 'Admin Demo account controls are not connected.')));
  const statusModal = document.getElementById('statusModal');
  function closeStatusModal() { statusModal.hidden = true; }
  document.getElementById('statusAction').addEventListener('click', () => {
    const suspend = member.status !== 'Suspended';
    document.getElementById('statusModalTitle').textContent = suspend ? `Suspend ${member.name}?` : `Reactivate ${member.name}?`;
    document.getElementById('statusModalCopy').textContent = suspend ? 'This sample account will be marked suspended on this page only. No real account access will change.' : 'This sample account will be marked active on this page only.';
    const confirm = document.getElementById('confirmStatus');
    confirm.textContent = suspend ? 'Suspend account' : 'Reactivate account';
    confirm.className = suspend ? 'button danger-button' : 'button primary';
    statusModal.hidden = false;
  });
  ['closeStatusModal','cancelStatus'].forEach(id => document.getElementById(id).addEventListener('click', closeStatusModal));
  statusModal.addEventListener('click', event => { if (event.target === statusModal) closeStatusModal(); });
  document.getElementById('confirmStatus').addEventListener('click', () => {
    member.status = member.status === 'Suspended' ? 'Active' : 'Suspended';
    updateStatusPill();
    document.getElementById('standingLabel').textContent = member.status === 'Suspended' ? 'Suspended' : 'Good standing';
    document.getElementById('standingLabel').className = member.status === 'Suspended' ? 'standing-bad' : 'standing-good';
    closeStatusModal();
    showToast(`${member.name} status changed in this demo view.`);
  });
  document.addEventListener('keydown', event => { if (event.key === 'Escape') { closeStatusModal(); closeSidebar(); } });
  const sidebar = document.getElementById('sidebar');
  const scrim = document.getElementById('sidebarScrim');
  function closeSidebar() { sidebar.classList.remove('open'); scrim.classList.remove('open'); }
  document.getElementById('mobileMenu').addEventListener('click', () => { sidebar.classList.toggle('open'); scrim.classList.toggle('open'); });
  scrim.addEventListener('click', closeSidebar);

  applyMember();
})();
