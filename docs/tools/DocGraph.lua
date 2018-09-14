
Class.DocGraph = {
	nodes = false,
}

function DocGraph:__ctor()
	self.nodes = { }
end

function DocGraph:AddFolders(folders)
	for i,folder in ipairs(folders) do
		self:AddFolder(folder)
	end
end

function DocGraph:AddFolder(folder)
	local files = io.listfiles(folder)
	for i,file in ipairs(files) do
		self:AddFile(file)
	end
end

function DocGraph:AddFile(parser, file)
	local new_nodes = parser:ParseFile(file)

	--update connections between new nodes
	for i,new_node_1 in ipairs(new_nodes) do
		for j,new_node_2 in ipairs(new_nodes) do
			if new_node_1 ~= new_node_2 then
				new_node_1:UpdateConnectionsWithNode(new_node_2)
				new_node_2:UpdateConnectionsWithNode(new_node_1)
			end
		end
	end

	--update connncetions between new nodes and old nodes
	for i,new_node in ipairs(new_nodes) do
		new_node.graph = self
		for j,old_node in ipairs(self.nodes) do
			old_node:UpdateConnectionsWithNode(new_node)
			new_node:UpdateConnectionsWithNode(old_node)
		end
	end

	--try merge with an old node or just add it
	for i,new_node in ipairs(new_nodes) do
		local merge_success
		for j,old_node in ipairs(self.nodes) do
			merge_success = old_node:TryMergeNode(new_node)
			if merge_success then break end
		end

		if not merge_success then
			table.insert(self.nodes, new_node)
		end
	end
end

function DocGraph:FindNode(title)
	for i,node in ipairs(self.nodes) do
		if node.title == title then
			return node
		end
	end
end
