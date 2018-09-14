
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

	for i,node in ipairs(new_nodes) do
		node.graph = self
	end

	for i,node in ipairs(self.nodes) do
		for i,new_node in ipairs(new_nodes) do
			node:UpdateConnectionsWithNode(new_node)
			new_node:UpdateConnectionsWithNode(node)
		end
	end

	for i,node in ipairs(new_nodes) do
		table.insert(self.nodes, node)
	end
end

function DocGraph:FindNode(title)
	for i,node in ipairs(self.nodes) do
		if node.title == title then
			return node
		end
	end
end
