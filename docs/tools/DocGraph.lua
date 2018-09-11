
Class.DocGraph = {
	parser = false,
	nodes = false,
	links = false,
}

function DocGraph:__ctor(parser)
	self.parser = parser
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

function DocGraph:AddFile(file)
	local new_nodes = self.parser:ParseFile(file)
	for i,node in ipairs(new_nodes) do
		table.insert(self.nodes, node)
	end

	--TODO update links
end
