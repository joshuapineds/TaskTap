(() => {
  const toast = document.getElementById('toast');
  const rows = document.getElementById('memberRows');
  const pageSize = 8;
  let timer;
  let currentPage = 1;
  let activeStatus = 'all';
  let pendingAction = null;
  const members = [
    { id:'USR-2486', name:'Camille Santos', email:'camille.santos@example.com', initials:'CS', color:'peach', role:'Both', barangay:'Brgy. 176', joined:'2026-10-11', tasks:2, verification:'Verified', status:'Active', phone:'+63 917 ••• 0184', completed:2, reports:0 },
    { id:'USR-2485', name:'Paolo Mendoza', email:'paolo.mendoza@example.com', initials:'PM', color:'mint', role:'Task performer', barangay:'Brgy. 178', joined:'2026-10-10', tasks:0, verification:'Pending', status:'Pending', phone:'+63 918 ••• 2640', completed:0, reports:0 },
    { id:'USR-2484', name:'Trisha Bautista', email:'trisha.bautista@example.com', initials:'TB', color:'violet', role:'Task poster', barangay:'Brgy. 179', joined:'2026-10-09', tasks:5, verification:'Verified', status:'Active', phone:'+63 905 ••• 3381', completed:4, reports:0 },
    { id:'USR-2483', name:'Mark Villanueva', email:'mark.villanueva@example.com', initials:'MV', color:'blue', role:'Both', barangay:'Brgy. 176', joined:'2026-10-08', tasks:11, verification:'Verified', status:'Active', phone:'+63 917 ••• 5502', completed:9, reports:0 },
    { id:'USR-2482', name:'Janelle Cruz', email:'janelle.cruz@example.com', initials:'JC', color:'rose', role:'Task performer', barangay:'Brgy. 178', joined:'2026-10-07', tasks:3, verification:'Unverified', status:'Active', phone:'+63 926 ••• 7190', completed:2, reports:0 },
    { id:'USR-2481', name:'Rafael Dizon', email:'rafael.dizon@example.com', initials:'RD', color:'gold', role:'Task poster', barangay:'Brgy. 179', joined:'2026-10-06', tasks:6, verification:'Pending', status:'Pending', phone:'+63 917 ••• 6285', completed:1, reports:1 },
    { id:'USR-2480', name:'Bea Flores', email:'bea.flores@example.com', initials:'BF', color:'sky', role:'Both', barangay:'Brgy. 176', joined:'2026-10-05', tasks:8, verification:'Verified', status:'Active', phone:'+63 908 ••• 1476', completed:7, reports:0 },
    { id:'USR-2479', name:'Noel Garcia', email:'noel.garcia@example.com', initials:'NG', color:'lilac', role:'Task performer', barangay:'Brgy. 178', joined:'2026-10-04', tasks:1, verification:'Unverified', status:'Suspended', phone:'+63 919 ••• 4207', completed:0, reports:2 },
    { id:'USR-2478', name:'Mika Reyes', email:'mika.reyes@example.com', initials:'MR', color:'mint', role:'Both', barangay:'Brgy. 179', joined:'2026-10-03', tasks:9, verification:'Verified', status:'Active', phone:'+63 917 ••• 2931', completed:8, reports:0 },
    { id:'USR-2477', name:'Anton Lim', email:'anton.lim@example.com', initials:'AL', color:'peach', role:'Task poster', barangay:'Brgy. 176', joined:'2026-10-02', tasks:4, verification:'Verified', status:'Active', phone:'+63 915 ••• 7012', completed:3, reports:0 },
    { id:'USR-2476', name:'Lara Domingo', email:'lara.domingo@example.com', initials:'LD', color:'blue', role:'Task performer', barangay:'Brgy. 178', joined:'2026-10-01', tasks:0, verification:'Pending', status:'Pending', phone:'+63 916 ••• 8330', completed:0, reports:0 },
    { id:'USR-2475', name:'Enzo Ramirez', email:'enzo.ramirez@example.com', initials:'ER', color:'violet', role:'Both', barangay:'Brgy. 179', joined:'2026-09-30', tasks:12, verification:'Verified', status:'Suspended', phone:'+63 927 ••• 3415', completed:10, reports:3 }
  ];

  function notify(message) {
    toast.textContent = message;
    toast.classList.add('show');
    clearTimeout(timer);
    timer = setTimeout(() => toast.classList.remove('show'), 2800);
  }
  function escapeHTML(value) {
    return String(value).replace(/[&<>"']/g, character => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[character]));
  }
  function dateLabel(value) {
    return new Date(`${value}T00:00:00`).toLocaleDateString('en-PH', { day:'numeric', month:'short', year:'numeric' });
  }
  function filteredMembers() {
    const query = document.getElementById('memberSearch').value.trim().toLowerCase();
    const role = document.getElementById('roleFilter').value;
    const verification = document.getElementById('verificationFilter').value;
    const sort = document.getElementById('sortFilter').value;
    const matches = members.filter(member => {
      const searchMatch = !query || `${member.name} ${member.email} ${member.id}`.toLowerCase().includes(query);
      return searchMatch && (activeStatus === 'all' || member.status === activeStatus) && (role === 'all' || member.role === role) && (verification === 'all' || member.verification === verification);
    });
    if (sort === 'name') matches.sort((a,b) => a.name.localeCompare(b.name));
    if (sort === 'oldest') matches.sort((a,b) => a.joined.localeCompare(b.joined));
    if (sort === 'recent') matches.sort((a,b) => b.joined.localeCompare(a.joined));
    if (sort === 'tasks') matches.sort((a,b) => b.tasks - a.tasks);
    return matches;
  }
  function roleClass(role) {
    return role === 'Both' ? 'both-role' : role === 'Task poster' ? 'poster-role' : 'performer-role';
  }
  function render() {
    const matches = filteredMembers();
    const totalPages = Math.max(1, Math.ceil(matches.length / pageSize));
    currentPage = Math.min(currentPage, totalPages);
    const start = (currentPage - 1) * pageSize;
    const visible = matches.slice(start, start + pageSize);
    rows.innerHTML = visible.map(member => {
      const verifyClass = member.verification.toLowerCase();
      const statusClass = member.status.toLowerCase() + '-status';
      return `<tr data-id="${escapeHTML(member.id)}">
        <td class="check-column"><input class="row-select" type="checkbox" aria-label="Select ${escapeHTML(member.name)}"></td>
        <td><div class="member-info"><span class="member-avatar ${escapeHTML(member.color)}">${escapeHTML(member.initials)}</span><span><strong>${escapeHTML(member.name)}</strong><small>${escapeHTML(member.id)} · ${escapeHTML(member.email)}</small></span></div></td>
        <td><span class="role-pill ${roleClass(member.role)}">${escapeHTML(member.role)}</span></td>
        <td>${escapeHTML(member.barangay)}</td><td>${dateLabel(member.joined)}</td><td class="task-count">${member.tasks}</td>
        <td><span class="verification-pill ${verifyClass}">${escapeHTML(member.verification)}</span></td>
        <td><span class="status-pill ${statusClass}">${escapeHTML(member.status)}</span></td>
        <td><button class="member-actions" data-view="${escapeHTML(member.id)}" aria-label="View ${escapeHTML(member.name)} details">···</button></td>
      </tr>`;
    }).join('');
    document.getElementById('emptyState').hidden = matches.length > 0;
    document.querySelector('.member-table').hidden = matches.length === 0;
    const summary = matches.length
      ? `Showing ${start + 1}–${Math.min(start + pageSize, matches.length)} of ${matches.length} sample records (${(2486 + members.length - 12).toLocaleString()} total members)`
      : 'No sample records match these filters';
    document.getElementById('resultSummary').textContent = summary;
    document.getElementById('pageLabel').textContent = `Page ${currentPage} of ${totalPages}`;
    document.getElementById('currentPage').textContent = currentPage;
    document.getElementById('prevPage').disabled = currentPage <= 1;
    document.getElementById('nextPage').disabled = currentPage >= totalPages;
    document.getElementById('selectAll').checked = false;
    updateSelected();
    document.getElementById('clearFilters').hidden = !(document.getElementById('memberSearch').value || activeStatus !== 'all' || document.getElementById('roleFilter').value !== 'all' || document.getElementById('verificationFilter').value !== 'all');
  }
  function updateSelected() {
    const selected = rows.querySelectorAll('.row-select:checked').length;
    const target = document.getElementById('selectedInfo');
    target.textContent = selected ? `${selected} member${selected === 1 ? '' : 's'} selected` : 'Select members to export selected records.';
    target.classList.toggle('selected', selected > 0);
    document.getElementById('exportSelected').hidden = selected === 0;
  }
  function syncCounts() {
    const counts = members.reduce((result, member) => { result[member.status] = (result[member.status] || 0) + 1; return result; }, {});
    const total = 2486 + members.length - 12;
    document.getElementById('totalMembers').textContent = total.toLocaleString();
    document.getElementById('allTabCount').textContent = total.toLocaleString();
    document.getElementById('activeMembers').textContent = (2301 + (counts.Active || 0) - 7).toLocaleString();
    document.getElementById('pendingMembers').textContent = (126 + (counts.Pending || 0) - 3).toLocaleString();
    document.getElementById('suspendedMembers').textContent = (59 + (counts.Suspended || 0) - 2).toLocaleString();
    document.querySelector('[data-status="Active"] span').textContent = (2301 + (counts.Active || 0) - 7).toLocaleString();
    document.querySelector('[data-status="Pending"] span').textContent = (126 + (counts.Pending || 0) - 3).toLocaleString();
    document.querySelector('[data-status="Suspended"] span').textContent = (59 + (counts.Suspended || 0) - 2).toLocaleString();
  }
  function showModal(id) { document.getElementById(id).hidden = false; }
  function closeModals() { document.querySelectorAll('.modal-backdrop').forEach(modal => { modal.hidden = true; }); pendingAction = null; }
  function viewMember(id) {
    const member = members.find(item => item.id === id);
    if (!member) return;
    const verifyClass = member.verification.toLowerCase();
    const statusClass = member.status.toLowerCase() + '-status';
    document.getElementById('memberModalContent').innerHTML = `<div class="member-detail-head"><span class="member-avatar detail-avatar ${escapeHTML(member.color)}">${escapeHTML(member.initials)}</span><span><h2 id="memberModalTitle">${escapeHTML(member.name)}</h2><p>${escapeHTML(member.id)} · Joined ${dateLabel(member.joined)}</p></span></div>
      <div class="detail-badges"><span class="role-pill ${roleClass(member.role)}">${escapeHTML(member.role)}</span><span class="verification-pill ${verifyClass}">${escapeHTML(member.verification)}</span><span class="status-pill ${statusClass}">${escapeHTML(member.status)}</span></div>
      <div class="detail-grid"><div class="detail-field"><span>Email address</span><strong>${escapeHTML(member.email)}</strong></div><div class="detail-field"><span>Phone</span><strong>${escapeHTML(member.phone)}</strong></div><div class="detail-field"><span>Barangay</span><strong>${escapeHTML(member.barangay)}</strong></div><div class="detail-field"><span>Tasks posted / joined</span><strong>${member.tasks}</strong></div><div class="detail-field"><span>Tasks completed</span><strong>${member.completed}</strong></div><div class="detail-field"><span>Reports received</span><strong>${member.reports}</strong></div></div>
      <div class="detail-note"><span>ⓘ</span><span>Sample data for the A-02 prototype. Open A-03 User Detail for account activity and history.</span></div>
      <div class="modal-actions"><button class="button secondary" data-close-modal>Close</button><button class="button secondary" id="openUserDetail">Open full detail</button><button class="button ${member.status === 'Suspended' ? 'primary' : 'danger-button'}" id="toggleMemberStatus" data-member="${escapeHTML(member.id)}">${member.status === 'Suspended' ? 'Reactivate' : 'Suspend'}</button></div>`;
    showModal('memberModal');
    document.querySelectorAll('#memberModalContent [data-close-modal]').forEach(button => button.addEventListener('click', closeModals));
    document.getElementById('toggleMemberStatus').addEventListener('click', () => confirmStatus(member.id));
    document.getElementById('openUserDetail').addEventListener('click', () => {
      try { sessionStorage.setItem('tasktapAdminSelectedMember', JSON.stringify(member)); } catch { /* Demo detail falls back to bundled sample data. */ }
      closeModals();
      window.location.href = `[Admin] user-detail.html?id=${encodeURIComponent(member.id)}`;
    });
  }
  function confirmStatus(id) {
    const member = members.find(item => item.id === id);
    if (!member) return;
    pendingAction = id;
    const willSuspend = member.status !== 'Suspended';
    document.getElementById('confirmTitle').textContent = willSuspend ? `Suspend ${member.name}?` : `Reactivate ${member.name}?`;
    document.getElementById('confirmCopy').textContent = willSuspend ? 'This sample account will be marked suspended in this page only. No account access will change.' : 'This sample account will be marked active in this page only.';
    const button = document.getElementById('confirmAction');
    button.textContent = willSuspend ? 'Suspend account' : 'Reactivate account';
    button.classList.toggle('danger-button', willSuspend);
    button.classList.toggle('primary', !willSuspend);
    showModal('confirmModal');
  }
  function downloadCSV(list, filename) {
    const columns = [['Member ID','Name','Email','Role','Barangay','Joined','Tasks','Verification','Status']];
    const data = list.map(member => [member.id,member.name,member.email,member.role,member.barangay,member.joined,member.tasks,member.verification,member.status]);
    const csv = columns.concat(data).map(row => row.map(value => `"${String(value).replaceAll('"','""')}"`).join(',')).join('\r\n');
    const url = URL.createObjectURL(new Blob([csv], {type:'text/csv;charset=utf-8'}));
    const anchor = document.createElement('a'); anchor.href = url; anchor.download = filename; anchor.click(); URL.revokeObjectURL(url);
    notify(`${list.length} sample member record${list.length === 1 ? '' : 's'} exported.`);
  }

  document.querySelectorAll('.future-link').forEach(link => link.addEventListener('click', event => { event.preventDefault(); notify(`${link.dataset.page || 'This page'} is planned for a future admin milestone.`); }));
  document.querySelectorAll('[data-toast]').forEach(link => link.addEventListener('click', event => { event.preventDefault(); notify(link.dataset.toast); }));
  ['helpButton','notificationButton','profileButton','adminProfile'].forEach(id => document.getElementById(id).addEventListener('click', () => notify(id === 'notificationButton' ? 'No new member alerts in this demo.' : 'Admin Demo account controls are not connected.')));
  document.querySelectorAll('[data-close-modal]').forEach(button => button.addEventListener('click', closeModals));
  document.querySelectorAll('.modal-backdrop').forEach(modal => modal.addEventListener('click', event => { if (event.target === modal) closeModals(); }));
  document.addEventListener('keydown', event => { if (event.key === 'Escape') closeModals(); if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') { event.preventDefault(); document.getElementById('memberSearch').focus(); } });

  document.querySelectorAll('.member-tab').forEach(tab => tab.addEventListener('click', () => {
    activeStatus = tab.dataset.status;
    currentPage = 1;
    document.querySelectorAll('.member-tab').forEach(item => { item.classList.toggle('active', item === tab); item.setAttribute('aria-selected', item === tab ? 'true' : 'false'); });
    render();
  }));
  ['memberSearch','roleFilter','verificationFilter','sortFilter'].forEach(id => document.getElementById(id).addEventListener(id === 'memberSearch' ? 'input' : 'change', () => { currentPage = 1; render(); }));
  document.getElementById('clearFilters').addEventListener('click', () => {
    document.getElementById('memberSearch').value = '';
    document.getElementById('roleFilter').value = 'all';
    document.getElementById('verificationFilter').value = 'all';
    activeStatus = 'all'; currentPage = 1;
    document.querySelectorAll('.member-tab').forEach(tab => { const active = tab.dataset.status === 'all'; tab.classList.toggle('active', active); tab.setAttribute('aria-selected', active ? 'true' : 'false'); });
    render();
  });
  document.getElementById('prevPage').addEventListener('click', () => { currentPage--; render(); });
  document.getElementById('nextPage').addEventListener('click', () => { currentPage++; render(); });
  document.getElementById('selectAll').addEventListener('change', event => { rows.querySelectorAll('.row-select').forEach(input => { input.checked = event.target.checked; }); updateSelected(); });
  rows.addEventListener('change', event => { if (event.target.matches('.row-select')) { const checkboxes = [...rows.querySelectorAll('.row-select')]; document.getElementById('selectAll').checked = checkboxes.length > 0 && checkboxes.every(input => input.checked); updateSelected(); } });
  rows.addEventListener('click', event => { const button = event.target.closest('[data-view]'); if (button) viewMember(button.dataset.view); });
  document.getElementById('exportButton').addEventListener('click', () => downloadCSV(members, 'tasktap-demo-members.csv'));
  document.getElementById('exportSelected').addEventListener('click', () => {
    const ids = [...rows.querySelectorAll('.row-select:checked')].map(input => input.closest('tr').dataset.id);
    downloadCSV(members.filter(member => ids.includes(member.id)), 'tasktap-demo-selected-members.csv');
  });
  document.getElementById('addUserButton').addEventListener('click', () => { document.getElementById('addMemberForm').reset(); showModal('addModal'); });
  document.getElementById('addMemberForm').addEventListener('submit', event => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const name = String(form.get('name')).trim();
    const email = String(form.get('email')).trim();
    const initials = name.split(/\s+/).slice(0,2).map(part => part[0]).join('').toUpperCase();
    const id = `USR-${2487 + members.length - 12}`;
    members.unshift({ id, name, email, initials, color:'sky', role:form.get('role'), barangay:form.get('barangay'), joined:'2026-10-11', tasks:0, verification:'Unverified', status:'Active', phone:'Not provided', completed:0, reports:0 });
    syncCounts(); activeStatus = 'all'; currentPage = 1;
    document.querySelectorAll('.member-tab').forEach(tab => { const active = tab.dataset.status === 'all'; tab.classList.toggle('active', active); tab.setAttribute('aria-selected', active ? 'true' : 'false'); });
    closeModals(); render(); notify('Sample member added to this page.');
  });
  document.getElementById('confirmAction').addEventListener('click', () => {
    const member = members.find(item => item.id === pendingAction);
    if (!member) return;
    member.status = member.status === 'Suspended' ? 'Active' : 'Suspended';
    syncCounts(); closeModals(); render(); notify(`${member.name} updated in this demo view.`);
  });

  const menu = document.getElementById('mobileMenu');
  const sidebar = document.getElementById('sidebar');
  const scrim = document.getElementById('sidebarScrim');
  function closeSidebar() { sidebar.classList.remove('open'); scrim.classList.remove('open'); }
  menu.addEventListener('click', () => { sidebar.classList.toggle('open'); scrim.classList.toggle('open'); });
  scrim.addEventListener('click', closeSidebar);
  render();
})();
