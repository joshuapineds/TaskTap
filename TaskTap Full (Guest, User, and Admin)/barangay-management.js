(() => {
  const areas = [
    { id: 'BRY-0178', name: 'Brgy. 178', city: 'Caloocan City', region: 'Metro Manila', coordinator: 'Marisol Reyes', members: 192, tasks: 6, status: 'Active', reviewed: '2026-10-08' },
    { id: 'BRY-0179', name: 'Brgy. 179', city: 'Caloocan City', region: 'Metro Manila', coordinator: 'Paolo Mendoza', members: 238, tasks: 9, status: 'Active', reviewed: '2026-10-09' },
    { id: 'BRY-0180', name: 'Brgy. 180', city: 'Caloocan City', region: 'Metro Manila', coordinator: 'Carmen Lim', members: 176, tasks: 7, status: 'Active', reviewed: '2026-10-05' },
    { id: 'BRY-0181', name: 'Brgy. 181', city: 'Caloocan City', region: 'Metro Manila', coordinator: 'Not assigned', members: 151, tasks: 4, status: 'Needs review', reviewed: '2026-08-22' },
    { id: 'BRY-0182', name: 'Brgy. 182', city: 'Caloocan City', region: 'Metro Manila', coordinator: 'Jasper Cruz', members: 164, tasks: 5, status: 'Active', reviewed: '2026-10-01' },
    { id: 'BRY-0183', name: 'Brgy. 183', city: 'Caloocan City', region: 'Metro Manila', coordinator: 'Not assigned', members: 97, tasks: 2, status: 'Needs review', reviewed: '2026-08-15' },
    { id: 'BRY-0184', name: 'Brgy. 184', city: 'Caloocan City', region: 'Metro Manila', coordinator: 'Nina Flores', members: 185, tasks: 8, status: 'Active', reviewed: '2026-10-10' },
    { id: 'BRY-0185', name: 'Brgy. 185', city: 'Caloocan City', region: 'Metro Manila', coordinator: 'Alex Santos', members: 81, tasks: 5, status: 'Inactive', reviewed: '2026-07-30' }
  ];
  const el = id => document.getElementById(id);
  const tbody = el('barangayRows');
  const state = { query: '', status: 'all', sort: 'name', editingId: null };
  let nextId = 186;
  let toastTimer;
  const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[char]));
  const dateLabel = value => new Date(`${value}T00:00:00`).toLocaleDateString('en-PH', { day: 'numeric', month: 'short', year: 'numeric' });
  const stateClass = value => value.toLowerCase().replace(/\s+/g, '-');
  function toast(message) {
    const box = el('toast'); box.textContent = message; box.classList.add('show');
    clearTimeout(toastTimer); toastTimer = window.setTimeout(() => box.classList.remove('show'), 2500);
  }
  function visibleAreas() {
    const query = state.query.trim().toLowerCase();
    return areas.filter(area => (state.status === 'all' || area.status === state.status)
      && (!query || [area.name, area.city, area.region, area.coordinator, area.id].join(' ').toLowerCase().includes(query)))
      .sort((a, b) => state.sort === 'tasks' ? b.tasks - a.tasks
        : state.sort === 'members' ? b.members - a.members
          : state.sort === 'coverage' ? a.status.localeCompare(b.status) || a.name.localeCompare(b.name)
            : a.name.localeCompare(b.name, undefined, { numeric: true }));
  }
  function renderInsights() {
    const active = areas.filter(area => area.status === 'Active');
    const top = [...areas].sort((a, b) => b.tasks - a.tasks)[0];
    const review = areas.filter(area => area.status === 'Needs review');
    const growth = areas.find(area => area.name === 'Brgy. 184') || active[0];
    const totalMembers = areas.reduce((sum, area) => sum + Number(area.members || 0), 0);
    const totalTasks = areas.reduce((sum, area) => sum + Number(area.tasks || 0), 0);
    el('statAreas').textContent = areas.length;
    el('statMembers').textContent = totalMembers.toLocaleString('en-PH');
    el('statTasks').textContent = totalTasks.toLocaleString('en-PH');
    el('statCoverage').textContent = `${active.length} / ${areas.length}`;
    el('coverageActiveLabel').textContent = `${active.length} active ${active.length === 1 ? 'area' : 'areas'}`;
    el('topAreaTitle').textContent = `${top?.name || 'No area'} has the most open tasks`;
    el('topAreaCopy').textContent = `${top?.tasks || 0} active listings · strongest local demand`;
    el('reviewInsightTitle').textContent = `${review.length} ${review.length === 1 ? 'area needs' : 'areas need'} a coverage review`;
    el('growthInsightTitle').textContent = `${growth?.name || 'No area'} is growing`;
    el('directoryCount').textContent = `${areas.length} ${areas.length === 1 ? 'area' : 'areas'}`;
    el('coverageBlocks').innerHTML = areas.slice(0, 8).map(area => `<button type="button" class="map-area ${stateClass(area.status)}" data-edit="${escapeHtml(area.id)}" aria-label="Edit ${escapeHtml(area.name)} · ${escapeHtml(area.status)}"><strong>${escapeHtml(area.name)}</strong><small>${area.tasks} open ${area.tasks === 1 ? 'task' : 'tasks'}</small></button>`).join('');
  }
  function renderDirectory() {
    const list = visibleAreas();
    tbody.innerHTML = list.map(area => `<tr><td><div class="area-name-cell"><span class="area-mini-icon">⌖</span><div><strong>${escapeHtml(area.name)}</strong><small>${escapeHtml(area.city)}, ${escapeHtml(area.region)} · ${escapeHtml(area.id)}</small></div></div></td><td class="coordinator-cell">${escapeHtml(area.coordinator)}${area.coordinator === 'Not assigned' ? '<small>Coordinator needed</small>' : '<small>Local contact</small>'}</td><td class="number-cell">${Number(area.members).toLocaleString('en-PH')} <small>members</small></td><td class="number-cell">${Number(area.tasks).toLocaleString('en-PH')} <small>open</small></td><td><span class="coverage-status ${stateClass(area.status)}">${escapeHtml(area.status)}</span></td><td class="reviewed-cell">${dateLabel(area.reviewed)}</td><td><div class="area-actions"><button class="area-action" data-edit="${escapeHtml(area.id)}">Edit</button><button class="area-action ${area.status === 'Inactive' ? '' : 'remove'}" data-toggle="${escapeHtml(area.id)}">${area.status === 'Inactive' ? 'Activate' : 'Deactivate'}</button></div></td></tr>`).join('');
    el('barangayEmpty').hidden = list.length !== 0;
    tbody.closest('table').hidden = list.length === 0;
    el('directoryResult').textContent = `Showing ${list.length} service ${list.length === 1 ? 'area' : 'areas'}`;
    el('directoryFooter').textContent = `${list.length} sample ${list.length === 1 ? 'area' : 'areas'}`;
    el('clearBarangayFilters').hidden = !state.query && state.status === 'all';
  }
  function render() { renderInsights(); renderDirectory(); }
  function openForm(area = null) {
    state.editingId = area?.id || null;
    el('barangayModalTitle').textContent = area ? 'Edit service area' : 'Add barangay';
    el('saveBarangay').textContent = area ? 'Save changes' : 'Save area';
    el('barangayId').value = area?.id || '';
    el('barangayName').value = area?.name || '';
    el('barangayCity').value = area?.city || 'Manila';
    el('barangayRegion').value = area?.region || 'Metro Manila';
    el('barangayCoordinator').value = area?.coordinator === 'Not assigned' ? '' : (area?.coordinator || '');
    el('barangayFormStatus').value = area?.status || 'Active';
    el('barangayMembers').value = area?.members ?? 0;
    el('barangayTasks').value = area?.tasks ?? 0;
    el('barangayFormError').textContent = '';
    el('barangayModal').hidden = false;
    window.setTimeout(() => el('barangayName').focus(), 0);
  }
  function closeModal() { el('barangayModal').hidden = true; }
  el('addBarangay').addEventListener('click', () => openForm());
  el('barangayForm').addEventListener('submit', event => {
    event.preventDefault();
    const name = el('barangayName').value.trim();
    const duplicate = areas.some(area => area.name.toLowerCase() === name.toLowerCase() && area.id !== state.editingId);
    if (duplicate) { el('barangayFormError').textContent = 'A service area with this name already exists.'; return; }
    const area = {
      id: state.editingId || `BRY-${String(nextId++).padStart(4, '0')}`,
      name, city: el('barangayCity').value.trim(), region: el('barangayRegion').value.trim(),
      coordinator: el('barangayCoordinator').value.trim() || 'Not assigned',
      status: el('barangayFormStatus').value, members: Number(el('barangayMembers').value || 0),
      tasks: Number(el('barangayTasks').value || 0), reviewed: new Date().toISOString().slice(0, 10)
    };
    const index = areas.findIndex(item => item.id === state.editingId);
    if (index >= 0) areas[index] = area; else areas.push(area);
    closeModal(); render(); toast(`${area.name} ${index >= 0 ? 'updated' : 'added'} in this demo.`);
  });
  el('barangayRows').addEventListener('click', event => {
    const edit = event.target.closest('[data-edit]'); const toggle = event.target.closest('[data-toggle]');
    if (edit) { const area = areas.find(item => item.id === edit.dataset.edit); if (area) openForm(area); }
    if (toggle) {
      const area = areas.find(item => item.id === toggle.dataset.toggle); if (!area) return;
      area.status = area.status === 'Inactive' ? 'Active' : 'Inactive'; area.reviewed = new Date().toISOString().slice(0, 10);
      render(); toast(`${area.name} marked ${area.status.toLowerCase()} in this demo.`);
    }
  });
  el('coverageBlocks').addEventListener('click', event => { const button = event.target.closest('[data-edit]'); if (button) { const area = areas.find(item => item.id === button.dataset.edit); if (area) openForm(area); } });
  el('barangaySearch').addEventListener('input', event => { state.query = event.target.value; renderDirectory(); });
  el('barangayStatus').addEventListener('change', event => { state.status = event.target.value; renderDirectory(); });
  el('barangaySort').addEventListener('change', event => { state.sort = event.target.value; renderDirectory(); });
  el('clearBarangayFilters').addEventListener('click', () => {
    state.query = ''; state.status = 'all'; state.sort = 'name';
    el('barangaySearch').value = ''; el('barangayStatus').value = 'all'; el('barangaySort').value = 'name'; renderDirectory();
  });
  el('showReviewAreas').addEventListener('click', () => { el('barangayStatus').value = 'Needs review'; state.status = 'Needs review'; renderDirectory(); el('barangayDirectory')?.scrollIntoView({ behavior: 'smooth', block: 'start' }); });
  el('exportBarangays').addEventListener('click', () => {
    const headers = ['ID', 'Barangay', 'City', 'Region', 'Coordinator', 'Active members', 'Open tasks', 'Coverage status', 'Last reviewed'];
    const data = [headers, ...visibleAreas().map(area => [area.id, area.name, area.city, area.region, area.coordinator, area.members, area.tasks, area.status, area.reviewed])];
    const csv = data.map(row => row.map(value => `"${String(value).replace(/"/g, '""')}"`).join(',')).join('\r\n');
    const url = URL.createObjectURL(new Blob([csv], { type: 'text/csv;charset=utf-8' }));
    const link = document.createElement('a'); link.href = url; link.download = 'tasktap-barangay-directory-demo.csv'; link.click(); URL.revokeObjectURL(url);
  });
  document.querySelectorAll('[data-close-modal]').forEach(button => button.addEventListener('click', closeModal));
  el('barangayModal').addEventListener('click', event => { if (event.target === el('barangayModal')) closeModal(); });
  document.addEventListener('keydown', event => {
    if (event.key === 'Escape') closeModal();
    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') { event.preventDefault(); el('barangaySearch').focus(); }
  });
  document.querySelectorAll('.future-link, [data-toast]').forEach(link => link.addEventListener('click', event => { event.preventDefault(); toast(link.dataset.toast || `${link.dataset.page} is planned as a future admin page.`); }));
  el('helpButton')?.addEventListener('click', () => toast('Barangay coverage and directory actions are sample data for this frontend demo.'));
  el('notificationButton')?.addEventListener('click', () => toast('No new barangay coverage alerts in this demo.'));
  const sidebar = el('sidebar'); const scrim = el('sidebarScrim');
  el('mobileMenu')?.addEventListener('click', () => { sidebar.classList.add('open'); scrim.classList.add('open'); });
  scrim?.addEventListener('click', () => { sidebar.classList.remove('open'); scrim.classList.remove('open'); });
  render();
})();
